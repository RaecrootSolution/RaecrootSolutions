using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ICSI_Library.Util
{
    public abstract class SaltKeyHandler
    {

        public static void SaltKeyAdd(ref List<SaltKeyEntry> ObjSaltKey, string lStrSalt)
        {
            SaltKeyEntry tmpEnty = new SaltKeyEntry();
            try
            {
                tmpEnty.UserHostAddress = HttpContext.Current.Request.UserHostAddress;
                tmpEnty.SaltKeyValue = lStrSalt;
                DateTime dt = new DateTime();
                dt = DateTime.Now.AddMinutes(5);
                tmpEnty.ExpireTime = dt;
                ObjSaltKey.Add(tmpEnty);

            }
            catch (Exception er)
            {
                ///errLabel.InnerText = "Please Refresh the page";
                //Logger.Logerror(er);
            }
            finally
            {
                /// Write the method how will delete the exipred salt key data.
                SaltKeyRemoveExpire(ref ObjSaltKey);
            }
        }

        public static string SaltKeyGet(ref List<SaltKeyEntry> ObjSaltKey)
        {
            string hostip = HttpContext.Current.Request.UserHostAddress;
            SaltKeyEntry tmpSltky = new SaltKeyEntry();
            string rtnVal = "";
            try
            {
                tmpSltky = ObjSaltKey.Find(delegate (SaltKeyEntry o) { return o.UserHostAddress == hostip.ToString(); });
                if (tmpSltky != null)
                {
                    rtnVal = tmpSltky.SaltKeyValue;
                }
            }
            catch (Exception er)
            {
                //Logger.Logerror(er);
            }
            finally
            {
                tmpSltky = null;
                hostip = null;
                SaltKeyRemoveExpire(ref ObjSaltKey);
            }
            return rtnVal;


        }


        private static void SaltKeyRemoveExpire(ref List<SaltKeyEntry> ObjSaltKey)
        {
            //SaltKeyEntry tmpSltkyobj = new SaltKeyEntry();
            List<SaltKeyEntry> tmpSltkyobj;
            try
            {
                tmpSltkyobj = ObjSaltKey.FindAll(delegate (SaltKeyEntry o)
                { return o.ExpireTime >= DateTime.Now; });

                if (tmpSltkyobj != null)
                {
                    ObjSaltKey = tmpSltkyobj;
                }
            }
            catch (Exception er)
            {
                //Logger.Logerror(er);
            }
            finally
            {
                tmpSltkyobj = null;
            }

        }

        public static void SaltKeyRemove(ref List<SaltKeyEntry> ObjSaltKey)
        {
            string strHostIp = HttpContext.Current.Request.UserHostAddress;
            //ObjSaltKey.Remove(new SaltKeyEntry  );
            List<SaltKeyEntry> tmpSltkyobj;
            try
            {
                tmpSltkyobj = ObjSaltKey.FindAll(delegate (SaltKeyEntry o)
                { return o.UserHostAddress != strHostIp; });

                if (tmpSltkyobj != null)
                {
                    ObjSaltKey = tmpSltkyobj;
                }
            }
            catch (Exception er)
            {
                //Logger.Logerror(er);
            }
            finally
            {
                tmpSltkyobj = null;
            }

        }

        public static void updateCaptcha(ref List<SaltKeyEntry> ObjSaltKey, string captchString)
        {
            //SaltKeyEntry Newtmp12 = new SaltKeyEntry();
            string strHostIp = HttpContext.Current.Request.UserHostAddress;
            try
            {
                foreach (SaltKeyEntry Newtmp in ObjSaltKey)
                {
                    if (Newtmp.UserHostAddress == strHostIp)
                    {
                        //ListofItem.Remove(int.Parse(RowNumberId));
                        Newtmp.CaptchaValue = captchString;
                    }

                }
            }
            catch (Exception er)
            {
                //Logger.Logerror(er);
            }
        }


        public static string GetCaptchValue(ref List<SaltKeyEntry> ObjSaltKey)
        {
            string hostip = HttpContext.Current.Request.UserHostAddress;
            SaltKeyEntry tmpSltky = new SaltKeyEntry();
            string rtnVal = "";
            try
            {
                tmpSltky = ObjSaltKey.Find(delegate (SaltKeyEntry o) { return o.UserHostAddress == hostip.ToString(); });

                if (tmpSltky != null)
                {
                    rtnVal = tmpSltky.CaptchaValue;
                }
            }
            catch (Exception er)
            {
                //Logger.Logerror(er);
            }
            finally
            {
                tmpSltky = null;
                hostip = null;
                SaltKeyRemoveExpire(ref ObjSaltKey);
            }
            return rtnVal;
        }
    }
}
