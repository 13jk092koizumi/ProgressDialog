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
    [Activity(Label = "AsyncProgressDialogActivity", MainLauncher = true)]
    public class AsyncProgressDialogActivity : Activity {
        public const string MyScanAction = "MY_SCAN_ACTION";
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ProgressDialogLayout);

            ProgressDialog progressDialog = new ProgressDialog(this);
            progressDialog.SetTitle("�X�L������");
            progressDialog.SetMessage("���΂炭���҂��b��������...");
            progressDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
            
            Button button = FindViewById<Button>(Resource.Id.scan_button);
            button.Click += delegate {
                var filter = new IntentFilter();
                filter.AddAction(MyScanAction);
                var receiver = new ScanReceiver(progressDialog);
                RegisterReceiver(receiver, filter);
                SendBroadcast(new Intent(MyScanAction));
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
                var action = intent.Action;
                if (!action.Equals(AsyncProgressDialogActivity.MyScanAction)) {
                    Console.WriteLine("can't receieve myscanaction!");
                }
                exeThread();
                context.UnregisterReceiver(this);
            }

            private void exeThread() {
                thread = new Thread(() => {
                    Handler handler = new Handler(context.MainLooper);
                    
                    handler.Post(() => { progressDialog.Show(); });

                    WifiManager wifi = (WifiManager)context.GetSystemService(WifiService);
                    for (int i = 1; i <= 10; i++) {
                        handler.Post(() => {
                            progressDialog.Progress = i * 10; progressDialog.SetMessage(string.Format("����{0}��", i * 10)); //�񓯊��̂������œ��I��Message��ς����܂��B
                        });
                        wifi.StartScan();

                        /*DB�փf�[�^��ǉ����鏈���Ȃ�*/

                        Thread.Sleep(1000);//���U�Ńv���O�������I������Ⴄ�̂�
                        Message msg = new Message();
                        msg.Obj = string.Format("HandleMessage:i={0}",i);
                        handler.SendMessage(msg);
                    }
                    
                    handler.Post(() => { progressDialog.Dismiss(); });
                    handler.Post(() => { Toast.MakeText(context, "�X�L�����I��", ToastLength.Short).Show(); });
                });
                thread.Start();
            }
        }
    }
}