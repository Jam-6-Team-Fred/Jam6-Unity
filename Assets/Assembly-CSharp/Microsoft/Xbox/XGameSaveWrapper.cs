using System.Collections.Generic;

namespace Microsoft.Xbox
{
	public class XGameSaveWrapper
	{
		public delegate void InitializeCallback(int hresult);

		public delegate void GetQuotaCallback(int hresult, long remainingQuota);

		public delegate void QueryContainersCallback(int hresult, string[] containerNames);

		public delegate void QueryBlobsCallback(int hresult, Dictionary<string, uint> blobInfos);

		public delegate void LoadCallback(int hresult, byte[] blobData);

		public delegate void SaveCallback(int hresult, string blobName);

		public delegate void DeleteCallback(int hresult);

		public delegate void DeleteBlobCallback(int hresult, string name);

		private delegate void UpdateCallback(int hresult, string blobName);

		~XGameSaveWrapper()
		{
		}

		public void GetQuotaAsync(GetQuotaCallback callback)
		{
			callback(0, 0L);
		}

		public void QueryContainers(string containerNamePrefix, QueryContainersCallback callback)
		{
			callback(0, new string[0]);
		}

		public void QueryContainerBlobs(string containerName, QueryBlobsCallback callback)
		{
			callback(0, new Dictionary<string, uint>());
		}

		public void Load(string containerName, string blobName, LoadCallback callback)
		{
			callback(0, new byte[0]);
		}

		public void Save(string containerName, string blobName, byte[] blobData, SaveCallback callback)
		{
			callback(0, blobName);
		}

		public void Delete(string containerName, DeleteCallback callback)
		{
			callback(0);
		}

		public void Delete(string containerName, string blobName, DeleteBlobCallback callback)
		{
			callback(0, blobName);
		}

		public void Delete(string containerName, string[] blobNames, DeleteBlobCallback callback)
		{
			callback(0, string.Join(",", blobNames));
		}

		private void Update(string containerName, IDictionary<string, byte[]> blobsToSave, IList<string> blobsToDelete, UpdateCallback callback)
		{
			callback(0, "");
		}
	}
}
