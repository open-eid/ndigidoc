// Copyright (C) AS Sertifitseerimiskeskus
// This software is released under the BSD License (see LICENSE.BSD)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;

using NDigiDoc;
using NDigiDocUtility.Operations;

namespace NDigiDocUtility
{
    public class NDigiDocUtility
    {
        public static void Main(String[] args)
        {
            // ourExe.exe -cdoc-recipient testimiseks.pem -cdoc-encrypt tocdoc.txt tocdoc1.cdoc
            /*
            args = new string[]
                       {
                           "-cdoc-encrypt",
                           @"C:\Users\John\Desktop\.NET_4.0.rar",
                           @"C:\Users\John\Desktop\net4",

                           "-cdoc-recipient",
                           @"C:\Users\John\Desktop\testimiseks.pfx?1234"
                       };
            */
            
            /*
            args = new string[]
                       {
                           "-cdoc-recipient", 
                           @"C:\Users\John\Desktop\b4b.p12?b4b", 
                        
   
                           "-cdoc-encrypt", 
                           @"C:\Users\John\Desktop\Hardcore.txt", 
                           @"C:\Users\John\Desktop\Hardcore.cdoc"
                       };
           */
           
            //YourExe.exe -cdoc-in tocdoc2.cdoc -cdoc-decrypt-pkcs12 testimiseks.pfx 1234 PKCS12 decrypted.txt
            
            /*
            args = new string[]
                       {
                           "-cdoc-in", 
                           @"C:\Users\John\Desktop\fromSwed.xml", 
                           //@"C:\Users\John\Desktop\NDigiDoc.cdoc",

                           "-cdoc-decrypt-pkcs12", 
                           @"C:\Users\John\Desktop\uptime.p12", 
                           @"uptime",
                           "PKCS12",
                           @"C:\Users\John\Desktop\fromSwedWithLoveAndDecrypted.txt"
                       };
            */

            if(args.Length == 0)
            {
                DisplayHelpText();
                return;
            }

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                new ArgumentHandler(args, FindImplementedOperations()).Execute();
                stopWatch.Stop();
                Console.WriteLine("Done! Time elapsed: " + string.Format("{0:00} min {1:00} sec {2:00} ms", stopWatch.Elapsed.Minutes, stopWatch.Elapsed.Seconds, stopWatch.Elapsed.Milliseconds));
            }
            catch (Exception e)
            {
                WL(e.ToString());
            }
        }

        static AbstractOperation[] FindImplementedOperations()
        {
            // Get types where element x is 1) A class 2) A class inside NDigiDocUtility.Operations namespace 3) A class that is not abstract 4) Class inherits from AbstractOperation
            var types = (from x in Assembly.GetExecutingAssembly().GetTypes()
                         where x.IsClass && x.Namespace.Equals("NDigiDocUtility.Operations") && !x.IsAbstract && typeof(AbstractOperation).IsAssignableFrom(x)
                         select x).ToArray();

            // Create instances of found operations
            var operations = new AbstractOperation[types.Length];
            for (int i = 0; i != types.Length; ++i)
            {
                operations[i] = Activator.CreateInstance(types[i]) as AbstractOperation;
            }
            return operations;
        }

        /// <summary>
        /// Displays the parameter explanations with examples.
        /// </summary>
        static void DisplayHelpText()
        {
            WL("Standard syntax: ");
            WL("");
            WL(" -cdoc-decrypt-pkcs12  <cert-input-uri> <password> <keystore-type> <output-uri>");
            WL(" -cdoc-encrypt         <input-uri> <output-uri>");
            WL(" -cdoc-in              <input-uri>");
            WL(" -cdoc-recipient       <<certificate-uri> <certificate-uri> ..> ");
            WL("");
            WL("NDigiDoc specific syntax: ");
            WL("");
            WL(" -cdoc-recipient       <<certificate-uri>?<password> ..>");
            WL(" -log4net-xmlconfig-in <log4net-xml-config-uri> ");


            WL("");
            WL("[Example] Decrypt CDoc : \nNDigiDoc.exe \n-cdoc-in ToDecrypt.cdoc \n-cdoc-decrypt-pkcs12 MyCert.pfx 12345 PKCS12 C:\\Users\\John\\Desktop\\decrypted.txt");
            WL("");
            WL("[Example] Create new CDoc: \nNDigiDoc.exe  \n-cdoc-recipient C:\\Recipient1Cert.pem C:\\Recipient2Cert.pfx Recipient3Cert.p12 \n-cdoc-encrypt tocdoc.txt tocdoc1.cdoc");
            WL("");
        }

        /// <summary>
        /// Shorthand Console.WriteLine
        /// </summary>
        /// <param name="textToWrite"></param>
        static void WL(string textToWrite)
        {
            Console.WriteLine(textToWrite);
        }

        /// <summary>
        /// Shorthand Console.Write
        /// </summary>
        /// <param name="textToWrite"></param>
        static void W(string textToWrite)
        {
            Console.Write(textToWrite);
        }
    }
}
