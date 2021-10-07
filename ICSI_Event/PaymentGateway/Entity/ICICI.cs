using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using PaymentGateway.Entity.Interface;

namespace PaymentGateway.Entity
{
    public class ICICI : IPaymentGatewaySource
    {
        public string GetRequestData(PaymentTransactionEntity objPmtEntity, PaymentGatewayMaster objPG)
        {
            string _transdata = string.Empty;
            string _returnurl = string.Empty;
            //_returnurl = objPG.ResponseURL + "&Reference No="+objPmtEntity.RequestID.ToString()+"&submerchantid=" + objPG.SubMerchantID
            //    + "&transaction amount=" + objPmtEntity.Amount.ToString("0.00")
            //    + "&paymode="+objPmtEntity.PaymentMode+"";// + objPmtEntity.PaymentMode;
            //_transdata = ""+objPmtEntity.RequestID.ToString()+"|" + objPG.SubMerchantID + "|" + objPmtEntity.Amount.ToString("0.00") + "|Amit|"
            //    + objPG.MobNo + "&optional fields=abc@icici&returnurl=" + _returnurl;
            //objPmtEntity.RequestData = objPG.GatewayURL + "?merchantid=" + objPG.MerchantId + "&mandatory fields=" + _transdata;
            _returnurl = objPG.ResponseURL + "&Reference No=" + objPmtEntity.RequestID.ToString() + "&submerchantid=" + objPG.SubMerchantID
                + "&transaction amount=" + objPmtEntity.Amount.ToString("0.00")
                + "&paymode=" + objPmtEntity.PaymentMode + "";// + objPmtEntity.PaymentMode;
            _transdata = "" + objPmtEntity.RequestID.ToString() + "|" + objPG.SubMerchantID + "|" + objPmtEntity.Amount.ToString("0.00") + "|" + objPG.Name + "|"
                + objPG.MobNo + "&optional fields=abc@icici&returnurl=" + _returnurl;
            objPmtEntity.RequestData = objPG.GatewayURL + "?merchantid=" + objPG.MerchantId + "&mandatory fields=" + _transdata;
            return objPmtEntity.RequestData;
        }
        public string GetEncryptedRequestData(PaymentTransactionEntity objPmtEntity, PaymentGatewayMaster objPG)
        {
            string _transdata = string.Empty;
            string _returnurl = string.Empty;
            //_returnurl = encrypttext(objPG.CheckSumKey, objPG.ResponseURL) + "&Reference No=" + encrypttext(objPG.CheckSumKey, objPmtEntity.RequestID.ToString())
            //    + "&submerchantid=" + encrypttext(objPG.CheckSumKey, objPG.SubMerchantID)
            //    + "&transaction amount=" + encrypttext(objPG.CheckSumKey, objPmtEntity.Amount.ToString("0.00"))
            //    + "&paymode=" + encrypttext(objPG.CheckSumKey, objPmtEntity.PaymentMode);
            //_transdata = encrypttext(objPG.CheckSumKey, objPmtEntity.RequestID.ToString() + "|" + objPG.SubMerchantID + "|" + objPmtEntity.Amount.ToString("0.00") + "|Amit|"
            //    + objPG.MobNo) + "&optional fields=&returnurl=" + _returnurl;
            _returnurl = encrypttext(objPG.CheckSumKey, objPG.ResponseURL) + "&Reference No=" + encrypttext(objPG.CheckSumKey, objPmtEntity.RequestID.ToString())
               + "&submerchantid=" + encrypttext(objPG.CheckSumKey, objPG.SubMerchantID)
               + "&transaction amount=" + encrypttext(objPG.CheckSumKey, objPmtEntity.Amount.ToString("0.00"))
               + "&paymode=" + encrypttext(objPG.CheckSumKey, objPmtEntity.PaymentMode);
            _transdata = encrypttext(objPG.CheckSumKey, objPmtEntity.RequestID.ToString() + "|" + objPG.SubMerchantID + "|" + objPmtEntity.Amount.ToString("0.00") + "|" + objPG.Name + "|"
                + objPG.MobNo) + "&optional fields=&returnurl=" + _returnurl;
            string RequestData = objPG.GatewayURL + "?merchantid=" + objPG.MerchantId + "&mandatory fields=" + _transdata;
            return RequestData;
        }
        public string encrypttext(string key, string textToEncrypt)
        {
            try
            {
                RijndaelManaged rijndaelCipher = new RijndaelManaged();
                rijndaelCipher.Mode = CipherMode.ECB;
                rijndaelCipher.Padding = PaddingMode.PKCS7;
                rijndaelCipher.KeySize = 0x80;
                rijndaelCipher.BlockSize = 0x80;
                byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
                byte[] keyBytes = new byte[0x10];
                int len = pwdBytes.Length;
                if (len > keyBytes.Length)
                {
                    len = keyBytes.Length;
                }
                Array.Copy(pwdBytes, keyBytes, len);
                rijndaelCipher.Key = keyBytes;
                rijndaelCipher.IV = keyBytes;
                ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
                byte[] plainText = Encoding.UTF8.GetBytes(textToEncrypt);
                return Convert.ToBase64String(transform.TransformFinalBlock(plainText, 0, plainText.Length));
            }
            catch
            {
                return "";
            }
        }
    }
}
