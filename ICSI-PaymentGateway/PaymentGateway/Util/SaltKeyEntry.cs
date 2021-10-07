using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICSI_Library.Util
{
    [Serializable]
    public class SaltKeyEntry
    {
        //

        private string _userhostaddress;
        private string _saltkeyvalue;
        private DateTime _expiretime;
        private string _sucessfullogin;
        private string _captchavalue;

        public string UserHostAddress
        {
            get { return _userhostaddress; }
            set { _userhostaddress = value; }
        }


        public string SaltKeyValue
        {
            get { return _saltkeyvalue; }
            set { _saltkeyvalue = value; }
        }

        public DateTime ExpireTime
        {
            get { return _expiretime; }
            set { _expiretime = value; }
        }

        public string SucessfulLogin
        {
            get { return _sucessfullogin; }
            set { _sucessfullogin = value; }
        }

        public string CaptchaValue
        {
            get { return _captchavalue; }
            set { _captchavalue = value; }
        }


    }
}
