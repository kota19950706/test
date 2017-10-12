//ハンドモデルにセットする．

//ゆらぎの生成
namespace GiveFluctuation
{
    #region//using
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    #endregion

    #region//構造体
    /// <summary>
    /// ゆらぎのパラメータ構造体
    /// </summary>
    public struct MyFluctuation
    {
        public Vector3 OutOfControl;    //xyz, correspond to Nocontorl~
        public bool[] UpflagXYZ;    //[3], correspond to Upflag~
        public bool[] fluctuationflag;  //[3], correspond to yuragiflag~
        public Vector3 fluctuation; //xyz, correspond to yuragi~
        //const float fingerfluctuation = 0.57f;  //correspond to yuragiFinger

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="x">ゆらぎ初期値</param>
        /// <param name="y">ゆらぎ初期値</param>
        /// <param name="z">ゆらぎ初期値</param>
        public MyFluctuation(float x, float y, float z)
        {
            this.OutOfControl = new Vector3();
            this.UpflagXYZ = new Boolean[3];  //規定値はfalse
            this.fluctuationflag = new Boolean[3] { true, true, true };
            this.fluctuation = new Vector3(x, y, z);    //0.52f, 0.48f, 0.42f
        }
    }
    #endregion

    /// <summary>
    /// ゆらぎの生成とVHの移動
    /// ※VHの初期座標は(0,0,0)であることが望ましい(変な動きをしない)
    /// </summary>
    public class HandFluctuation : MonoBehaviour
    {
        #region//変数宣言
        const float MAX_TH = 0.85f;     //間欠カオス法のリセット閾値上限
        const float MIN_TH = 0.15f;     //間欠カオス法のリセット閾値下限
        const int RE_NUM_MAX = 9;       //リセット乱数の上限(これの10分の1の値が実際に使用される)
        const int RE_NUM_MIN = 1;       //リセット乱数の下限(これの10分の1の値が実際に使用される)
        const float DIVIDE = 80.0f;     //ゆらぎ分割数
        const int X_RANGE = 65;         //x方向のレンジ
        const int Y_RANGE = 15;         //y方向のレンジ
        const int Z_RANGE = 70;         //z方向のレンジ
        //const float SPEED = 0.03f;      //ゆらぎの速度
        const int MAX_SPEED = 30;        //ゆらぎ最大速度(これの1000分の1の値が実際に使用される)
        const int MIN_SPEED = 20;        //ゆらぎ最小速度(これの1000分の1の値が実際に使用される)
        
        private MyFluctuation my_yuragi;
        private SetHandInfomation YuragiFlag;
        #endregion

        // Use this for initialization
        void Start()
        {
            //初期化
            my_yuragi = new MyFluctuation(0.52f, 0.21f, 0.42f);

            YuragiFlag = GetComponent<SetHandInfomation>();
       }

        // Update is called once per frame
        void Update()
        {
            if (YuragiFlag.TouchUpFlag)
               DiscreteChaos();    //間欠カオス法によるゆらぎを実施
        }

        #region//各種ゆらぎ計算関数

        void CalcYuragi(ref float yuragif)
        {
            if (yuragif < 0.5f)
                yuragif = yuragif + 2.0f * yuragif * yuragif;
            else
                yuragif = yuragif - 2.0f * (1.0f - yuragif) * (1.0f - yuragif);

            if (yuragif < MIN_TH || MAX_TH < yuragif)
            {
                System.Random r = new System.Random();
                yuragif = r.Next(RE_NUM_MIN, RE_NUM_MAX) / 10.0f;
                //Debug.Log(yuragif.ToString());    //OK
            }
        }

        void calcyuragianime(ref float yuragi, ref bool yuragiflag, ref bool Upflag, 
            ref float Nocontrol, int rangemax, int rangemin)   //rangeは揺らぐ範囲
        {
            if (yuragiflag)
            {
                CalcYuragi(ref yuragi);
                yuragiflag = false;
            }

            if (Upflag)
            {
                System.Random r = new System.Random();
                Nocontrol -= r.Next(MIN_SPEED, MAX_SPEED) / 1000.0f; //揺らぎ全体のスピードにつながる
                //Nocontrol -= SPEED;
                if (Nocontrol < rangemin * yuragi)
                {
                    Upflag = false;
                    yuragiflag = true;
                }
            }
            else
            {
                System.Random r = new System.Random();
                Nocontrol += r.Next(MIN_SPEED, MAX_SPEED) / 1000.0f; //揺らぎ全体のスピードにつながる
                //Nocontrol += SPEED;
                if (Nocontrol > rangemax * yuragi)
                    Upflag = true;
            }
        }

        void CalcYuragiAnime(ref MyFluctuation flu)
        {
            calcyuragianime(ref flu.fluctuation.x, ref flu.fluctuationflag[0], ref flu.UpflagXYZ[0], ref flu.OutOfControl.x, X_RANGE, -X_RANGE);
            calcyuragianime(ref flu.fluctuation.y, ref flu.fluctuationflag[1], ref flu.UpflagXYZ[1], ref flu.OutOfControl.y, Y_RANGE, -Y_RANGE);
            calcyuragianime(ref flu.fluctuation.z, ref flu.fluctuationflag[2], ref flu.UpflagXYZ[2], ref flu.OutOfControl.z, Z_RANGE, -Z_RANGE);
        }

        Vector3 DivideVec(Vector3 YuragiValue)
        {
            Vector3 temp;

            temp.x = YuragiValue.x / DIVIDE;
            temp.y = YuragiValue.y / DIVIDE;
            temp.z = YuragiValue.z / DIVIDE;

            return temp;
        }

        void DiscreteChaos()
        {   //this.transform.position += new Vector3(0.01f, 0, 0);    //test

            //ローカル座標を取得する
            my_yuragi.fluctuation = this.transform.position;
            
            //間欠カオス
            CalcYuragiAnime(ref my_yuragi);

            //更新
            this.transform.position += DivideVec(my_yuragi.OutOfControl);

        }

        //指の揺らぎも別途入れる？

        #endregion

    }
}
