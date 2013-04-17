﻿using azurecopy.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace azurecopy
{
    public class SkyDriveHandler : IBlobHandler
    {

        private string accessToken;

        public SkyDriveHandler()
        {
            accessToken = SkyDriveHelper.GetAccessToken();

        }

        public Blob ReadBlob(string url, string filePath = "")
        {
            Blob blob = null;


            var skydriveDirectoryEntry = SkyDriveHelper.GetSkyDriveEntryByFileNameAndDirectory(url);
            
            var requestUriFile =  new StringBuilder("https://apis.live.net/v5.0/"+skydriveDirectoryEntry.Id);
            requestUriFile.AppendFormat("?access_token={0}", accessToken);

            //byte[] arr = System.IO.File.ReadAllBytes("C:\\temp\\upload.txt");
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestUriFile.ToString());
            request.Method = "GET";
            //request.ContentType = "text/plain";
            //request.ContentLength = arr.Length;
            //Stream dataStream = request.GetRequestStream();
            //dataStream.Write(arr, 0, arr.Length);
            //dataStream.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var stream = response.GetResponseStream();

            byte[] arr = new byte[2000];
            stream.Read(arr, 0, 2000);
            var mystring = System.Text.Encoding.Default.GetString(arr);

            string returnString = response.StatusCode.ToString();

            blob.BlobOriginType = UrlType.Azure;
            return blob;
        }

        // url simply is <directory>/filename   format. NOT the entire/real url.
        public void WriteBlob(string url, Blob blob,  int parallelUploadFactor=1, int chunkSizeInMB=4)
        {

            var directoryName = GetDirectoryNameFromUrl( url );
            var blobName = GetBlobNameFromUrl(url);

            var directoryId = GetSkyDriveDirectoryId(directoryName);

            if (directoryId == null)
            {
                SkyDriveHelper.CreateFolder(directoryName);
                directoryId = GetSkyDriveDirectoryId(directoryName);
            }

            var urlTemplate = @"https://apis.live.net/v5.0/{0}/files/{1}";
            var requestUrl = string.Format(urlTemplate, directoryId, blobName);

            var requestUriFile = new StringBuilder(requestUrl);
            requestUriFile.AppendFormat("?access_token={0}", accessToken);
 
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestUriFile.ToString());
            request.Method = "PUT";
            Stream dataStream = request.GetRequestStream();
            Stream inputStream = null;

            // get stream to data.
            if (blob.BlobSavedToFile)
            {
                inputStream = new FileStream(blob.FilePath, FileMode.Open);
            }
            else
            {
                inputStream = new MemoryStream(blob.Data);
            }

            int bytesRead;
            int readSize = 64000;
            int totalSize = 0;
            byte[] arr = new byte[readSize];
            do
            {
                bytesRead = inputStream.Read( arr,0, readSize);
                if (bytesRead > 0)
                {
                    totalSize += bytesRead;
                    dataStream.Write(arr, 0, bytesRead);
                }
            }
            while (  bytesRead > 0);
         
            //request.ContentLength = totalSize;
         
            dataStream.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string returnString = response.StatusCode.ToString();

        }


        // assuming only single dir.
        // url == directory/blobname
        private string GetBlobNameFromUrl(string url)
        {
            var sp = url.Split('/');
            return sp[3];
        }

        // assuming only single dir.
        // url == directory/blobname
        private string GetDirectoryNameFromUrl(string url)
        {
            var sp = url.Split('/');
            return sp[2];
        }


        public List<string> ListBlobsInContainer(string container)
        {

            var skydriveListing = SkyDriveHelper.ListSkyDriveDirectory(container);

            // now just get list of names, and NOT the complete skydrive info.
            var nameList = (from e in skydriveListing select e.Name.ToString()).ToList();
            return nameList;

        }

        private string GetSkyDriveDirectoryId(string directoryName)
        {
            var skydriveListing = SkyDriveHelper.ListSkyDriveRootDirectories();

            var skydriveId = (from e in skydriveListing where e.Name == directoryName select e.Id).FirstOrDefault();

            return skydriveId;

        }




    }
}