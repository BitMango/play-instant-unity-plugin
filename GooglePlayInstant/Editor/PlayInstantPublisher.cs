// Copyright 2018 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.IO;
using GooglePlayInstant.Editor.GooglePlayServices;
using UnityEditor;
using UnityEngine;

namespace GooglePlayInstant.Editor
{
    /// <summary>
    /// Helper to build a ZIP file suitable for publishing on Play Console.
    /// </summary>
    public static class PlayInstantPublishser
    {
        private const string BaseApkFileName = "base.apk";

        /// <summary>
        /// Builds an APK and stores it in a user specified ZIP file.
        /// </summary>
        public static void Build()
        {
            var zipFilePath = EditorUtility.SaveFilePanel("Create APK in ZIP File", null, null, "zip");
            if (string.IsNullOrEmpty(zipFilePath))
            {
                // Assume cancelled.
                return;
            }

            var baseApkDirectory = Path.GetTempPath();
            var baseApkPath = Path.Combine(baseApkDirectory, BaseApkFileName);
            Debug.LogFormat("Building APK: {0}", baseApkPath);
            var buildPlayerOptions = PlayInstantBuilder.CreateBuildPlayerOptions(baseApkPath, BuildOptions.None);
            if (!PlayInstantBuilder.BuildAndSign(buildPlayerOptions))
            {
                // Do not log here. The method we called was responsible for logging.
                return;
            }

            // Zip creation is fast enough so call jar synchronously rather than wait for post build AppDomain reset.
            var arguments = string.Format(
                "cvf {0} -C {1} {2}",
                CommandLine.QuotePathIfNecessary(zipFilePath),
                CommandLine.QuotePathIfNecessary(baseApkDirectory),
                BaseApkFileName);
            var result = CommandLine.Run(JavaUtilities.JarBinaryPath, arguments);
            if (result.exitCode == 0)
            {
                Debug.LogFormat("Created ZIP containing base.apk: {0}", zipFilePath);
            }
            else
            {
                PlayInstantBuilder.LogError(string.Format("Zip creation failed: {0}", result.message));
            }
        }
    }
}