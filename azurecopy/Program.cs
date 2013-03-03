﻿﻿//-----------------------------------------------------------------------
// <copyright >
//    Copyright 2013 Ken Faulkner
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

using azurecopy.Helpers;
using azurecopy.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace azurecopy
{
    public enum UrlType { Azure, S3, Local };
    public enum Action { None, NormalCopy, BlobCopy, List }


    class Program
    {
        const string UsageString = "Usage: azurecopy -blobcopy -v -d <download directory> -i <inputUrl> -o <outputUrl> -list <inputUrl>\n    -list : Lists all blobs in given container/bucket\n    -blobcopy : Copy between input URL and output URL where output url HAS to be Azure\n    -v : verbose";
     
        const string LocalDetection = "???";
        const string VerboseFlag = "-v";
        const string InputUrlFlag = "-i";
        const string OutputUrlFlag = "-o";
        const string DownloadFlag = "-d";
        const string BlobCopyFlag = "-blobcopy";
        const string ListContainerFlag = "-list";


        // default access keys.
        const string AzureAccountKeyShortFlag = "-ak";
        const string AWSAccessKeyIDShortFlag = "-s3k";
        const string AWSSecretAccessKeyIDShortFlag = "-s3sk";

        const string AzureAccountKeyFlag = "-azurekey";
        const string AWSAccessKeyIDFlag = "-s3accesskey";
        const string AWSSecretAccessKeyIDFlag = "-s3secretkey";

        // source access keys
        const string SourceAzureAccountKeyShortFlag = "-sak";
        const string SourceAWSAccessKeyIDShortFlag = "-ss3k";
        const string SourceAWSSecretAccessKeyIDShortFlag = "-ss3sk";

        const string SourceAzureAccountKeyFlag = "-srcazurekey";
        const string SourceAWSAccessKeyIDFlag = "-srcs3accesskey";
        const string SourceAWSSecretAccessKeyIDFlag = "-srcs3secretkey";

        // target access keys
        const string TargetAzureAccountKeyShortFlag = "-tak";
        const string TargetAWSAccessKeyIDShortFlag = "-ts3k";
        const string TargetAWSSecretAccessKeyIDShortFlag = "-ts3sk";

        const string TargetAzureAccountKeyFlag = "-targetazurekey";
        const string TargetAWSAccessKeyIDFlag = "-targets3accesskey";
        const string TargetAWSSecretAccessKeyIDFlag = "-targets3secretkey";


        static UrlType _inputUrlType;
        static UrlType _outputUrlType;

        static string _inputUrl;
        static string _outputUrl;
        static string _downloadDirectory;
        static bool _verbose = false;
        static bool _amDownloading = false;
        static bool _useBlobCopy = false;
        static bool _listContainer = false;
        static Action _action = Action.None;

        /*
        // defaults
        static string _azureKey = String.Empty;
        static string _s3AccessKey = String.Empty;
        static string _s3SecretKey = String.Empty;

        // source
        static string _srcAzureKey = String.Empty;
        static string _srcS3AccessKey = String.Empty;
        static string _srcS3SecretKey = String.Empty;

        // target
        static string _targetAzureKey = String.Empty;
        static string _targetS3AccessKey = String.Empty;
        static string _targetS3SecretKey = String.Empty;
        */

        static string GetArgument(string[] args, int i)
        {
            if (i < args.Length)
            {
                return args[i];
            }
            else
            {
                throw new ArgumentException("Invalid parameters...");
            }

        }

        static UrlType GetUrlType(string url)
        {
            UrlType urlType = UrlType.Local;


            if ( AzureHelper.MatchHandler(url))
            {
                urlType = UrlType.Azure;
            }
            else if ( S3Helper.MatchHandler(url))
            {
                urlType = UrlType.S3;
            }

            return urlType;
        }

        static void ParseArguments(string[] args)
        {
            var i = 0;

            if (args.Length > 0)
            {
                while (i < args.Length)
                {
                    switch (args[i])
                    {
                        case VerboseFlag:
                            _verbose = true;
                            break;

                        case BlobCopyFlag:
                            _useBlobCopy = true;
                            _action = Action.BlobCopy;
                            break;


                        case ListContainerFlag:
                            i++;
                            _inputUrl = GetArgument(args, i);
                            _inputUrlType = GetUrlType(_inputUrl);
                            _listContainer = true;
                            _action = Action.List;

                            break;

                        case AzureAccountKeyFlag:
                        case AzureAccountKeyShortFlag:
                            i++;
                            var azureKey = GetArgument(args, i);
                            ConfigHelper.AzureAccountKey = azureKey;
                            ConfigHelper.SrcAzureAccountKey = azureKey;
                            ConfigHelper.TargetAzureAccountKey = azureKey;
                            break;

                        case AWSAccessKeyIDFlag:
                        case AWSAccessKeyIDShortFlag:

                            i++;
                            var s3AccessKey = GetArgument(args, i);
                            ConfigHelper.AWSAccessKeyID = s3AccessKey;
                            ConfigHelper.SrcAWSAccessKeyID = s3AccessKey;
                            ConfigHelper.TargetAWSAccessKeyID = s3AccessKey;
                            break;

                        case AWSSecretAccessKeyIDFlag:
                        case AWSSecretAccessKeyIDShortFlag:
                            i++;
                            var s3SecretKey = GetArgument(args, i);
                            ConfigHelper.AWSSecretAccessKeyID = s3SecretKey;
                            ConfigHelper.SrcAWSSecretAccessKeyID = s3SecretKey;
                            ConfigHelper.TargetAWSSecretAccessKeyID = s3SecretKey;

                            break;

                        case SourceAzureAccountKeyFlag:
                        case SourceAzureAccountKeyShortFlag:
                            i++;
                            var srcAzureKey = GetArgument(args, i);
                            ConfigHelper.SrcAzureAccountKey = srcAzureKey;
                            break;

                        case SourceAWSAccessKeyIDFlag:
                        case SourceAWSAccessKeyIDShortFlag:

                            i++;
                            var srcS3AccessKey = GetArgument(args, i);
                            ConfigHelper.SrcAWSAccessKeyID = srcS3AccessKey;

                            break;

                        case SourceAWSSecretAccessKeyIDFlag:
                        case SourceAWSSecretAccessKeyIDShortFlag:
                            i++;
                            var srcS3SecretKey = GetArgument(args, i);
                            ConfigHelper.SrcAWSSecretAccessKeyID = srcS3SecretKey;

                            break;

                        case TargetAzureAccountKeyFlag:
                        case TargetAzureAccountKeyShortFlag:
                            i++;
                            var targetAzureKey = GetArgument(args, i);
                            ConfigHelper.TargetAzureAccountKey = targetAzureKey;
                            break;

                        case TargetAWSAccessKeyIDFlag:
                        case TargetAWSAccessKeyIDShortFlag:

                            i++;
                            var targetS3AccessKey = GetArgument(args, i);
                            ConfigHelper.TargetAWSAccessKeyID = targetS3AccessKey;

                            break;

                        case TargetAWSSecretAccessKeyIDFlag:
                        case TargetAWSSecretAccessKeyIDShortFlag:
                            i++;
                            var targetS3SecretKey = GetArgument(args, i);
                            ConfigHelper.TargetAWSSecretAccessKeyID = targetS3SecretKey;

                            break;

                        case InputUrlFlag:
                            i++;
                            _inputUrl = GetArgument(args, i);
                            _inputUrlType = GetUrlType(_inputUrl);
                            if (_action == Action.None)
                            {
                                _action = Action.NormalCopy;
                            }
                            break;

                        case OutputUrlFlag:
                            i++;
                            _outputUrl = GetArgument(args, i);
                            _outputUrlType = GetUrlType(_outputUrl);
                            if (_action == Action.None)
                            {
                                _action = Action.NormalCopy;
                            }
                            break;

                        case DownloadFlag:
                            i++;
                            _downloadDirectory = GetArgument(args, i);
                            _amDownloading = true;
                            break;

                        default:
                            break;
                    }

                    i++;
                }
            }
            else
            {
                Console.WriteLine(UsageString);
            }

        }


        // default to local filesystem
        static IBlobHandler GetHandler(UrlType urlType)
        {
            IBlobHandler blobHandler;

            switch (urlType)
            {
                case UrlType.Azure:
                    blobHandler = new AzureHandler();
                    break;

                case UrlType.S3:
                    blobHandler = new S3Handler();
                    break;

                default:
                    blobHandler = new FileSystemHandler();
                    break;
            }

            return blobHandler;
        }



        static void DoNormalCopy()
        {

            IBlobHandler inputHandler = GetHandler(_inputUrlType);
            IBlobHandler outputHandler = GetHandler(_outputUrlType);


            if (inputHandler != null && outputHandler != null)
            {

                // handle multiple files.
                //currently sequentially.
                var sourceBlobList = GetSourceBlobList(inputHandler, _inputUrl);

                foreach (var url in sourceBlobList)
                {
                    var fileName = "";
                    if (_amDownloading)
                    {
                        fileName = GenerateFileName(_downloadDirectory, url);
                    }

                    var outputUrl = GenerateOutputUrl(_outputUrl, url);

                    if (!_useBlobCopy)
                    {

                        // read blob
                        var blob = inputHandler.ReadBlob(url, fileName);

                        // write blob
                        outputHandler.WriteBlob(outputUrl, blob);
                    }
                    else
                    {
                        Console.WriteLine("using blob copy {0} to {1}", url, outputUrl);
                        AzureBlobCopyHandler.StartCopy(url, outputUrl);
                    }


                }

            }
          
        }

        private static string GenerateOutputUrl(string baseOutputUrl, string inputUrl)
        {
            var u = new Uri(inputUrl);
            var blobName = "";
            blobName = u.Segments[u.Segments.Length - 1];

            var outputPath = Path.Combine(baseOutputUrl, blobName);

            return outputPath;

        }

        private static List<string> GetSourceBlobList(IBlobHandler inputHandler, string url)
        {
            var blobList = new List<string>();

            if (CommonHelper.IsABlob(url))
            {
                blobList.Add(url);
            }
            else
            {
                blobList = inputHandler.ListBlobsInContainer(url);
            }


            return blobList;
        }

        static void DoBlobCopy()
        {

            AzureBlobCopyHandler.StartCopy(_inputUrl, _outputUrl);

        }

        static void DoList()
        {
            IBlobHandler handler = GetHandler( _inputUrlType );
            var blobList = handler.ListBlobsInContainer(_inputUrl);

            foreach (var blob in blobList)
            {
                Console.WriteLine(blob);
            }

        }

        static void Process()
        {
            switch (_action)
            {
                case Action.List:
                    DoList();
                    break;
              
                case Action.NormalCopy:
                case Action.BlobCopy:
                    DoNormalCopy();
                    break;

                default:
                    break;
            }

        }


        private static string GenerateFileName(string downloadDirectory, string url)
        {
            var u = new Uri(url);
            var blobName = "";
            blobName = u.Segments[u.Segments.Length - 1];
            var fullPath = Path.Combine( downloadDirectory, blobName);

            return fullPath;
        }


        static void Main(string[] args)
        {
            ParseArguments(args);
            Process();
        }

    }
}
