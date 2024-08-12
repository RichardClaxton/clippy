using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipBoardManager
{
    internal class Clippy
    {
        public static int MAX_CLIPS = 9;
        public Dictionary<int, Clip> Clips { get; set; }

        /// <summary>
        /// Clippy
        /// </summary>
        public Clippy()
        {
            Clips = new Dictionary<int, Clip>();
        }

        /// <summary>
        /// addClip
        /// </summary>
        /// <param name="clipData"></param>
        public void addClip(String clipData)
        {
            var clip = new Clip()
            {
                capture = DateTime.Now,
                TypeOf = Clip.TYPE_OF_STRING,
                Data = clipData
            };

            Clips.Add(Clips.Count + 1,clip);
        }


        /// <summary>
        /// deleteClip
        /// </summary>
        /// <param name="id"></param>
        public void deleteClip(int id)
        {
            Clips.Remove(id);
            reIndexClips();
        }


        public void reIndexClips()
        {
            Dictionary<int, Clip> TempClips = Clips.ToDictionary(entry => entry.Key, entry => entry.Value);
            Clips.Clear();
            foreach (var clip in TempClips)
            {
                Clips.Add(Clips.Count + 1, clip.Value);
            }
            TempClips.Clear();
        }

        /// <summary>
        /// getNumberOfClips
        /// </summary>
        /// <returns></returns>
        public int getNumberOfClips()
        {
            return Clips.Count;
        }
    

        /// <summary>
        /// checkForClip
        /// </summary>
        /// <param name="clipData"></param>
        public Boolean checkForClip(String clipData)
        {
            return Clips.Values.Any(x => x.Data == clipData);
        }

        /// <summary>
        /// getClip
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public String getClip(int key)
        {
            try
            {
                return Clips[key].Data;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// saveClips
        /// </summary>
        public void saveClips()
        {
        // Serialize the Clips list to JSON
        string json = JsonConvert.SerializeObject(Clips, Formatting.Indented);

        // Convert JSON string to byte array
        byte[] stringBytes = Encoding.UTF8.GetBytes(json);

        // Create a MemoryStream for the compressed output
        using (var outputStream = new MemoryStream())
        {
            // Create a GZipStream for compression
            using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                // Write the JSON byte array to the GZipStream
                gzipStream.Write(stringBytes, 0, stringBytes.Length);
            }

            // Get the compressed data as byte array
            byte[] compressedBytes = outputStream.ToArray();

            // Save the compressed data to a file
            File.WriteAllBytes("clippy.data", compressedBytes);
        }
    }


        /// <summary>
        /// loadClips
        ///

        public bool loadClips()
        {
            try
            {
                // Read the compressed data from the file
                byte[] compressedData = File.ReadAllBytes("clippy.data");

                // Create a MemoryStream for the compressed input
                using (var inputStream = new MemoryStream(compressedData))
                {
                    // Create a GZipStream for decompression
                    using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                    {
                        // Create a MemoryStream for the decompressed output
                        using (var outputStream = new MemoryStream())
                        {
                            // Copy the decompressed data to the output stream
                            gzipStream.CopyTo(outputStream);

                            // Convert the decompressed byte array to a JSON string
                            byte[] decompressedBytes = outputStream.ToArray();
                            string json = Encoding.UTF8.GetString(decompressedBytes);

                            // Deserialize the JSON string back to the Clips list
                            Clips = JsonConvert.DeserializeObject<Dictionary<int, Clip>>(json);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

    }
}
