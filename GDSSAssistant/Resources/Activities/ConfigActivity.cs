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
using System.Threading;

namespace GDSSAssistant.Activities
{
    [Activity(Label = "ConfigActivity")]
    public class ConfigActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.ConfigForm);

            var btnBackConfigForm = FindViewById<Button>(Resource.Id.btnBackConfigForm);
            btnBackConfigForm.Click += delegate { Finish(); };

            GetConfigList();

            var lvEmail = FindViewById<ListView>(Resource.Id.lvEmailList);
            lvEmail.ItemClick += (sender, e) => {
                ConfigAdapter ConfigList = ((ConfigAdapter)lvEmail.Adapter);
                ConfigObject item = ConfigList[e.Position];

                var actEmalReader = new Intent(this, typeof(EmailReaderActivity));
                actEmalReader.PutExtra("ConfigName", item.Name);
                actEmalReader.PutExtra("ConfigValue", item.Value);
                actEmalReader.PutExtra("ConfigType", item.Type);
                actEmalReader.SetFlags(ActivityFlags.ReorderToFront);
                StartActivity(actEmalReader);
            };
        }

        private void GetConfigList()
        {
            DBManagerHelper dbmgr = new DBManagerHelper(this);
            DatabaseUpdates tblConfigList = new DatabaseUpdates();
            var eListAdapt = new ConfigAdapter(this, tblConfigList.GetConfigList());
            eListAdapt.NotifyDataSetChanged();

            var lvConfigList = FindViewById<ListView>(Resource.Id.lvConfigList);
            lvConfigList.Adapter = eListAdapt;

        }
    }

    public class ConfigObject
    {
        
        public string Name;
        public string Value;
        public string Type;
       
        public ConfigObject( string name, string value, string type)
        {
            this.Type = type;
            this.Name = name;
            this.Value = value;
        }

    }
    public class ConfigAdapter : BaseAdapter<ConfigObject>
    {
        List<ConfigObject> items;
        Activity context;
        public ConfigAdapter(Activity context, List<ConfigObject> items) : base()
        {
            this.context = context;
            this.items = items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override ConfigObject this[int position]
        {
            get { return items[position]; }
        }

        public override int Count
        {
            get
            {
                if (items == null)
                {
                    return 0;
                }
                else
                {
                    return items.Count() - 1;
                }
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = context.LayoutInflater.Inflate(Resource.Layout.ConfigList, null);
            var etxtNameConfigList = view.FindViewById<TextView>(Resource.Id.etxtNameConfigList);
            etxtNameConfigList.Text = item.Name;

            var etxtValueConfigList = view.FindViewById<TextView>(Resource.Id.etxtValueConfigList);
            etxtValueConfigList.Text = item.Value;

            return view;
        }
    }
}