using PrestaQi.Model.Spei;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;


namespace PrestaQi.Service.Tools
{
    public class CryptoHandler
    {
        public string ruta { get; set; }
        public string password { get; set; }

        public string sign(ordenPagoWS ordenPago, byte[] file)
        {
            string cadenaOriginal = originaString(ordenPago);
            X509Certificate2 x509 = new X509Certificate2(file, password);

            RSA rsa = (RSA)x509.PrivateKey;
            byte[] hashValue = rsa.SignData(System.Text.Encoding.UTF8.GetBytes(cadenaOriginal), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            string signature = System.Convert.ToBase64String(hashValue);

            return signature;
        }

        string originaString(ordenPagoWS ordenPago)
        {
            string cadenaOriginal = "";
            cadenaOriginal = cadenaOriginal + "||";
            cadenaOriginal = cadenaOriginal + ordenPago.institucionContraparte + "|";//1
            cadenaOriginal = cadenaOriginal + ordenPago.empresa + "|";//2
            cadenaOriginal = cadenaOriginal + zeroValidator(ordenPago.fechaOperacion) + "|";//3
            cadenaOriginal = cadenaOriginal + ordenPago.folioOrigen + "|";//4
            cadenaOriginal = cadenaOriginal + ordenPago.claveRastreo + "|";//5
            cadenaOriginal = cadenaOriginal + ordenPago.institucionOperante + "|";//6
            cadenaOriginal = cadenaOriginal + ordenPago.monto + "|";//7
            cadenaOriginal = cadenaOriginal + ordenPago.tipoPago + "|";//8
            cadenaOriginal = cadenaOriginal + zeroValidator(ordenPago.tipoCuentaOrdenante) + "|";//9
            cadenaOriginal = cadenaOriginal + ordenPago.nombreOrdenante + "|";//10
            cadenaOriginal = cadenaOriginal + ordenPago.cuentaOrdenante + "|";//11
            cadenaOriginal = cadenaOriginal + ordenPago.rfcCurpOrdenante + "|";//12
            cadenaOriginal = cadenaOriginal + ordenPago.tipoCuentaBeneficiario + "|";//13
            cadenaOriginal = cadenaOriginal + ordenPago.nombreBeneficiario + "|";//14
            cadenaOriginal = cadenaOriginal + ordenPago.cuentaBeneficiario + "|";//15
            cadenaOriginal = cadenaOriginal + ordenPago.rfcCurpBeneficiario + "|";//16
            cadenaOriginal = cadenaOriginal + ordenPago.emailBeneficiario + "|";//17
            cadenaOriginal = cadenaOriginal + zeroValidator(ordenPago.tipoCuentaBeneficiario2) + "|";//18
            cadenaOriginal = cadenaOriginal + ordenPago.nombreBeneficiario2 + "|";//19
            cadenaOriginal = cadenaOriginal + ordenPago.cuentaBeneficiario2 + "|";//20
            cadenaOriginal = cadenaOriginal + ordenPago.rfcCurpBeneficiario2 + "|";//21
            cadenaOriginal = cadenaOriginal + ordenPago.conceptoPago + "|";//22
            cadenaOriginal = cadenaOriginal + ordenPago.conceptoPago2 + "|";//23
            cadenaOriginal = cadenaOriginal + ordenPago.claveCatUsuario1 + "|";//24
            cadenaOriginal = cadenaOriginal + ordenPago.claveCatUsuario2 + "|";//25
            cadenaOriginal = cadenaOriginal + ordenPago.clavePago + "|";//26
            cadenaOriginal = cadenaOriginal + ordenPago.referenciaCobranza + "|";//27
            cadenaOriginal = cadenaOriginal + ordenPago.referenciaNumerica + "|";//28
            cadenaOriginal = cadenaOriginal + zeroValidator(ordenPago.tipoOperacion) + "|";//29
            cadenaOriginal = cadenaOriginal + ordenPago.topologia + "|";//30
            cadenaOriginal = cadenaOriginal + ordenPago.usuario + "|";//31
            cadenaOriginal = cadenaOriginal + zeroValidator(ordenPago.medioEntrega) + "|";//32
            cadenaOriginal = cadenaOriginal + zeroValidator(ordenPago.prioridad) + "|";//33
            cadenaOriginal = cadenaOriginal + zeroValidator(ordenPago.iva) + "|";//34
            cadenaOriginal = cadenaOriginal + "|";
            return cadenaOriginal;
        }

        public string zeroValidator(decimal number)
        {
            string finalValue = "";
            if (number != 0)
            {
                finalValue = number.ToString();
            }
            return finalValue;
        }
    
        public string signForSaldoCuenta(string cuentaClabe, byte[] file)
        {
            string cadenaOriginal = cuentaClabe;
            X509Certificate2 x509 = new X509Certificate2(file, password);

            RSA rsa = (RSA)x509.PrivateKey;
            byte[] hashValue = rsa.SignData(System.Text.Encoding.UTF8.GetBytes(cadenaOriginal), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            string signature = System.Convert.ToBase64String(hashValue);

            return signature;
        }

        public string signForConstaOrdenEnviadasRecibidas(string company, DateTime operatorDate, byte[] file)
        {
            string cadenaOriginal = $"|||{company}|{operatorDate.ToString("yyyyMMdd")}|||||||||||||||||||||||||||||||||";
            X509Certificate2 x509 = new X509Certificate2(file, password);

            RSA rsa = (RSA)x509.PrivateKey;
            byte[] hashValue = rsa.SignData(System.Text.Encoding.UTF8.GetBytes(cadenaOriginal), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            string signature = System.Convert.ToBase64String(hashValue);

            return signature;
        }
    }
}
