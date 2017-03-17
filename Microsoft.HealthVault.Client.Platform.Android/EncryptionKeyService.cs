using System;
using System.Threading.Tasks;
using Java.Security;
using System.IO;
using AndroidApp = Android.App;
using Javax.Crypto;
using Android.Provider;

namespace Microsoft.HealthVault.Client.Platform.Android
{
    /// <summary>
    /// Inplementation of IEncryptionKeyService
    /// </summary>
    internal class EncryptionKeyService : IEncryptionKeyService
    {
        private const string KeyStoreFileName = "HealthVault.Android.keystore";
        private AsyncLock keystoreLock = new AsyncLock();

        #region IEncryptionKeyService

        public async Task<byte[]> GetOrMakeEncryptionKeyAsync(string keyName)
        {
            using (await this.keystoreLock.LockAsync().ConfigureAwait(false))
            {
                byte[] key = await this.GetEncryptionKeyLockedAsync(keyName).ConfigureAwait(false);
                if (key == null)
                {
                    key = await this.CreateAndStoreEncryptionKeyLockedAsync(keyName).ConfigureAwait(false);
                }

                return key;
            }
        }

        public async Task<byte[]> GetEncryptionKeyAsync(string keyName)
        {
            using (await this.keystoreLock.LockAsync().ConfigureAwait(false))
            {
                return await this.GetEncryptionKeyLockedAsync(keyName).ConfigureAwait(false);
            }
        }

        public async Task<byte[]> CreateAndStoreEncryptionKeyAsync(string keyName)
        {
            using (await this.keystoreLock.LockAsync().ConfigureAwait(false))
            {
                return await this.CreateAndStoreEncryptionKeyLockedAsync(keyName).ConfigureAwait(false);
            }
        }

        #endregion

        #region Private

        private async Task<byte[]> GetEncryptionKeyLockedAsync(string keyName)
        {
            byte[] key = null;

            try
            {
                char[] deviceId = this.GetDeviceId().ToCharArray();
                KeyStore keyStore = await this.GetOrCreateKeyStoreAsync(deviceId).ConfigureAwait(false);

                KeyStore.IProtectionParameter protectionParameter = new KeyStore.PasswordProtection(deviceId);
                KeyStore.SecretKeyEntry secretKeyEntry = (KeyStore.SecretKeyEntry)keyStore.GetEntry(keyName, protectionParameter);

                if (secretKeyEntry != null)
                {
                    ISecretKey secretKey = secretKeyEntry.SecretKey;
                    if (secretKey != null)
                    {
                        key = secretKey.GetEncoded();
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // If the file isn't found, it's not a big deal and should mean it's just the first run.
                // The caller or the GetOrCreate method above will need to create it if we don't find it here.
            }

            return key;
        }

        private async Task<KeyStore> GetOrCreateKeyStoreAsync(char[] password)
        {
            KeyStore keyStore = KeyStore.GetInstance("BKS");
            try
            {
                using (Stream inputStream = await this.OpenKeyStoreFileForInputAsync().ConfigureAwait(false))
                {
                    keyStore.Load(inputStream, password);
                }
            }
            catch (Exception)
            {
                keyStore.Load(null);
            }

            return keyStore;
        }

        private async Task<byte[]> CreateAndStoreEncryptionKeyLockedAsync(string keyName)
        {
            byte[] keyBytes = null;

            KeyGenerator keyGenerator = KeyGenerator.GetInstance("AES");
            keyGenerator.Init(256, new SecureRandom());
            ISecretKey key = keyGenerator.GenerateKey();
            await this.StoreKeyAsync(keyName, key).ConfigureAwait(false);
            keyBytes = key.GetEncoded();

            return keyBytes;
        }

        private async Task StoreKeyAsync(string keyName, ISecretKey key)
        {
            char[] deviceId = this.GetDeviceId().ToCharArray();
            KeyStore keyStore = await this.GetOrCreateKeyStoreAsync(deviceId).ConfigureAwait(false);

            KeyStore.IProtectionParameter protectionParameter = new KeyStore.PasswordProtection(deviceId);

            // Create the SecretKeyEntry wrapper around the SecretKey
            KeyStore.SecretKeyEntry secretKeyEntry = new KeyStore.SecretKeyEntry(key);

            // Store the SecretKeyEntry in the KeyStore
            keyStore.SetEntry(keyName, secretKeyEntry, protectionParameter);

            // Write the KeyStore to the app's directory with appropriate permissions
            // Note: we're using the same key to protect the KeyStore as we are to protect the entry inside it.
            using (Stream fileStream = await this.OpenKeyStoreFileForOutputAsync().ConfigureAwait(false))
            {
                keyStore.Store(fileStream, deviceId);
            }
        }

        private Task<Stream> OpenKeyStoreFileForOutputAsync()
        {
            string directory = AndroidApp.Application.Context.NoBackupFilesDir.AbsolutePath;
            return Task.FromResult((Stream)File.Create(Path.Combine(directory, KeyStoreFileName)));
        }

        private Task<Stream> OpenKeyStoreFileForInputAsync()
        {
            string directory = AndroidApp.Application.Context.NoBackupFilesDir.AbsolutePath;
            string storeFile = Path.Combine(directory, KeyStoreFileName);
            return Task.FromResult((Stream)File.OpenRead(storeFile));
        }

        private string GetDeviceId()
        {
            var contentResolver = AndroidApp.Application.Context.ContentResolver;
            string deviceId = Settings.Secure.GetString(contentResolver, Settings.Secure.AndroidId);
            return deviceId;
        }

        #endregion
    }
}