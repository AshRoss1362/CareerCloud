﻿using CareerCloud.DataAccessLayer;
using CareerCloud.Pocos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CareerCloud.BusinessLogicLayer
{
    public class SystemLanguageCodeLogic : BaseLogicSystem<SystemLanguageCodePoco>
    {
        private const int saltLengthLimit = 10;

        public SystemLanguageCodeLogic(IDataRepository<SystemLanguageCodePoco> repository) : base(repository)
        {
        }


        //-------------------
        //public virtual SystemLanguageCodeLogic Get(string id)
        //{
        //    return _repository.GetSingle(c => c.LanguageID == id);
        //}
        //-------------------

        public bool Authenticate(string userName, string password)
        {
            SystemLanguageCodePoco poco = base.GetAll().Where(s => s.Login == userName).FirstOrDefault();
            if (null == poco)
            {
                return false;
            }
            return VerifyHash(password, poco.Password);
        }

        private bool VerifyHash(string password1, object password2)
        {
            throw new NotImplementedException();
        }

        public override void Add(SystemLanguageCodePoco[] pocos)
        {
            Verify(pocos);

            base.Add(pocos);
        }

        public override void Update(SystemLanguageCodePoco[] pocos)
        {
            Verify(pocos);
            base.Update(pocos);
        }

        protected override void Verify(SystemLanguageCodePoco[] pocos)
        {
            List<ValidationException> exceptions = new List<ValidationException>();
            string[] requiredExtendedPasswordChars = new string[] { "$", "*", "#", "_", "@" };

            foreach (var poco in pocos)
            {
                if (string.IsNullOrEmpty(poco.LanguageID))
                {
                    exceptions.Add(new ValidationException(1000, $"Language ID {poco.LanguageID} is required"));
                }
                if (string.IsNullOrEmpty(poco.Name))
                {
                    exceptions.Add(new ValidationException(1001, $"Name {poco.Name} is required"));
                }
                if (string.IsNullOrEmpty(poco.NativeName))
                {
                    exceptions.Add(new ValidationException(1002, $"Native name {poco.NativeName} is required"));
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        private static byte[] GetSalt()
        {
            return GetSalt(saltLengthLimit);
        }

        private static byte[] GetSalt(int maximumSaltLength)
        {
            var salt = new byte[maximumSaltLength];
            using (var random = new RNGCryptoServiceProvider())
            {
                random.GetNonZeroBytes(salt);
            }
            return salt;
        }

        private string ComputeHash(string plainText, byte[] saltBytes)
        {
            if (saltBytes == null)
            {
                saltBytes = GetSalt();
            }

            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] plainTextWithSaltBytes = new byte[plainTextBytes.Length + saltBytes.Length];
            for (int i = 0; i < plainTextBytes.Length; i++)
                plainTextWithSaltBytes[i] = plainTextBytes[i];

            for (int i = 0; i < saltBytes.Length; i++)
            {
                plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];
            }

            HashAlgorithm hash = new SHA512Managed();
            byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);
            byte[] hashWithSaltBytes = new byte[hashBytes.Length + saltBytes.Length];
            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashWithSaltBytes[i] = hashBytes[i];
            }
            for (int i = 0; i < saltBytes.Length; i++)
            {
                hashWithSaltBytes[hashBytes.Length + i] = saltBytes[i];
            }


            return Convert.ToBase64String(hashWithSaltBytes);
        }
        private bool VerifyHash(string plainText, string hashValue)
        {
            const int hashSizeInBytes = 64;

            byte[] hashWithSaltBytes = Convert.FromBase64String(hashValue);
            if (hashWithSaltBytes.Length < hashSizeInBytes)
                return false;
            byte[] saltBytes = new byte[hashWithSaltBytes.Length - hashSizeInBytes];
            for (int i = 0; i < saltBytes.Length; i++)
                saltBytes[i] = hashWithSaltBytes[hashSizeInBytes + i];
            string expectedHashString = ComputeHash(plainText, saltBytes);
            return (hashValue == expectedHashString);
        }

    }
    
}
