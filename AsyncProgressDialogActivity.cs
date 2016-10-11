using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Net.Wifi;


namespace GetWifi.src {
    [Activity(Label = "AsyncProgressDialogActivity", MainLauncher = true)] //c#の'属性'です。javaではいりません
    public class AsyncProgressDialogActivity : Activity {
        public const string MyScanAction = "MY_SCAN_ACTION"; //定数の文字列。const = final
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ProgressDialogLayout);
            //ProgressDialogの定義
            ProgressDialog progressDialog = new ProgressDialog(this);
            progressDialog.SetTitle("スキャン中");
            progressDialog.SetMessage("Message"); //後で変わるのでなんでもいい
            progressDialog.SetProgressStyle(ProgressDialogStyle.Horizontal); //大文字小文字がjavaと違いますが仕様です
            Button button = FindViewById<Button>(Resource.Id.scan_button);

            //ボタンのイベント。javaでいうonClick()です。
            button.Click += delegate {
                //BroadCastReceiverのブロードキャスト(アクション)を自作しています。便利そうなのでやってみました
                IntentFilter filter = new IntentFilter();
                filter.AddAction(MyScanAction); //ここはScanResulutsAvailableActionに変えてください
                ScanReceiver receiver = new ScanReceiver(progressDialog);
                RegisterReceiver(receiver, filter);
                SendBroadcast(new Intent(MyScanAction)); //Broadcast送信(自作のアクションのため)
            };
            
        }

        public class ScanReceiver : BroadcastReceiver {
            Thread thread;
            ProgressDialog progressDialog;
            Context context;
            public ScanReceiver(ProgressDialog dialog) {
                progressDialog = dialog;
            }
            public override void OnReceive(Context context, Intent intent) {
                this.context = context;
                exeThread(); //非同期処理実行
                context.UnregisterReceiver(this); //登録したら必ず解除する
            }

            //非同期処理。外に出したのに深い意味は無いです
            private void exeThread() {
                thread = new Thread(() => {
                    Handler handler = new Handler(context.MainLooper); //UIスレッドにアクセスするためHandlerを使います

                    handler.Post(() => { progressDialog.Show(); }); //javaだとRunnable()が必要だと思います
                    WifiManager wifi = (WifiManager)context.GetSystemService(WifiService);
                    
                    //i回スキャンする繰り返し処理
                    for (int i = 1; i <= 10; i++) {
                        handler.Post(() => {
                            progressDialog.Progress = i * 10; progressDialog.SetMessage(string.Format("現在{0}％", i * 10)); //非同期のおかげで動的にMessageを変えられます。
                        });
                        wifi.StartScan();

                        /*DBへデータを追加する処理などをここに*/

                        Thread.Sleep(1000);//速攻でプログラムが終わっちゃうので
                    }
                    
                    handler.Post(() => { progressDialog.Dismiss(); });
                    handler.Post(() => { Toast.MakeText(context, "スキャン終了", ToastLength.Short).Show(); });
                });
                thread.Start();
            }
        }
    }
}