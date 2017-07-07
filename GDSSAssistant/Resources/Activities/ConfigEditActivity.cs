using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using GDSSAssistant.DataManager;

namespace GDSSAssistant.Activities
{
    [Activity(Label = "ConfigEditActivity")]
    public class ConfigEditActivity : Activity
    {
        private String ConfigName;
        private String ConfigValue;
        private String ConfigType;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.ConfigEditor);

            ConfigName = Intent.GetStringExtra("ConfigName") ?? "From Data not available";
            ConfigValue = Intent.GetStringExtra("ConfigValue") ?? "Date Data not available";
            ConfigType = Intent.GetStringExtra("ConfigType") ?? "CC Data not available";

            var tvNameConfigEdit = FindViewById<TextView>(Resource.Id.tvNameConfigEdit);
            tvNameConfigEdit.Text = ConfigName;

            var etxtValueConfigEdit = FindViewById<EditText>(Resource.Id.etxtValueConfigEdit);
            etxtValueConfigEdit.Text = ConfigValue;

            var btnBackConfigEdit = FindViewById<Button>(Resource.Id.btnBackConfigEdit);
            btnBackConfigEdit.Click += delegate { Finish(); };

            var btnSaveConfigEdit = FindViewById<Button>(Resource.Id.btnSaveConfigEdit);
            btnSaveConfigEdit.Click += delegate {
                //save method here
                DBManagerHelper dbmgr = new DBManagerHelper(this);
                DatabaseUpdates tblConfigList = new DatabaseUpdates();
                tblConfigList.UpdateConfig(new DataManager.ConfigEntity() {
                    Name = ConfigName,
                    Value = ConfigValue,
                    Type = ConfigType
                } );
                dbmgr.Close();
                Toast.MakeText(this,"Configuration Update Successful",0);

            };

        }
    }
}