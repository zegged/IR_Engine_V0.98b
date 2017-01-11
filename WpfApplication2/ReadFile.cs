﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IR_Engine
{
    /// <summary>
    /// This class will read the document data.
    /// This class will "know" to get a path of the folder where all the files are in. (after the unzip)
    /// Every file has lots of documents. It is madetory to identify the beginning of every document
    /// and the separate them accordingly.
    /// 
    /// 
    /// 
    /// 
    /// https://en.wikipedia.org/wiki/Rabin%E2%80%93Karp_algorithm
    /// </summary>
    public class ReadFile
    {
      //  public static int wordPosition = 0;
        //UTF!!!
        public string filesPathToDelete;
        public static int totalDocs = 0;
       // private static List<Thread> ReadFileThreads;
        
       // private static Semaphore _ReadFileSemaphore;
        static int counter;




        public static SortedDictionary<string, string> OpenFileForParsing(string path)
        {
            Semaphore _ReadFileSemaphore = new Semaphore(2, 2) ;
            counter = 10;
            Mutex _myFilePostings = new Mutex();
            List<Thread> ReadFileThreads = new List<Thread>();
            List<SortedDictionary<string, string>> DicList = new List<SortedDictionary<string, string>>();
            SortedDictionary<string, string> myFilePostings = new SortedDictionary<string, string>();
            // Reference 1:
            //http://stackoverflow.com/questions/2161895/reading-large-text-files-with-streams-in-c-sharp

            int docNumber = 0;
            int linesInDoc = 0;
            string newDocument = String.Empty;
            //https://msdn.microsoft.com/en-us/library/system.text.stringbuilder(v=vs.110).aspx#StringAndSB
            StringBuilder bufferDocument = new StringBuilder();

            using (StreamReader sr = File.OpenText(path))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    //remove blank lines
                    if (s == "")
                        continue;
                    string term = "<DOC>";
                    int docidx = NaiveSearch(s, term);

                    if (docidx != -1) //new term found in line!
                    {
                        if (linesInDoc != 0) //end of document before
                        {
                     //       ReadFile.wordPosition = 0;
                            //      System.Console.WriteLine(newDocument);
                            
                            docNumber++;
                            totalDocs++;
                            //countAmountOfUniqueInDoc = 0;

                           // Console.WriteLine("Total Document #: " + Indexer.docNumber + 1);
                           // Console.WriteLine("File Document #: " + docNumber);
                          
                           // System.Console.WriteLine("Lines in document:" + linesInDoc);

                            //Indexer.docNumber++;
                            Indexer._DocNumber.WaitOne();
                            int freshNum = Interlocked.Increment(ref Indexer.docNumber);
                            Indexer._DocNumber.ReleaseMutex();

                            Console.WriteLine("Processed file :" + path + "| Found DOC#" + freshNum);
                            string str = bufferDocument.ToString();


                            Thread thread = new Thread(() => DoWork(ref _ReadFileSemaphore, str, freshNum, ref DicList));
                            // Start the thread, passing the number.

                            ReadFileThreads.Add(thread);
                        //    thread.Start();
                            //Dictionary<string, string> newDict = Parse.parseString(bufferDocument.ToString(), freshNum);
                            //dictionary resault

                            //remove metadata

                            //merge dic
                            //http://stackoverflow.com/questions/8459928/how-to-count-occurences-of-unique-values-in-dictionary
                            /*
                            System.Console.WriteLine("terms in document:" + newDict.Count);
                            System.Console.WriteLine("Merging in ReadFile...");

                            foreach (KeyValuePair<string, string> entry in newDict)
                                if (myFilePostings.ContainsKey(entry.Key))
                                    myFilePostings[entry.Key] += " " + entry.Value;// + "}@" + Indexer.docNumber;
                                else
                                    myFilePostings.Add(entry.Key.ToString(), entry.Value);// + "}@" + Indexer.docNumber);
                            System.Console.WriteLine("Merging in ReadFile... Done.");

                            System.Console.WriteLine("Deleteing string...");
                            //newDocument = String.Empty; //refresh string

                            //print original
                            */
                            bufferDocument.Clear();
                            // System.Console.WriteLine("Deleteing stringg... Done.");
                            //      printDic(myPostings);
                            //      Console.WriteLine("-------------------------------");
                            //           Console.WriteLine("Press any key to continue.");
                            //      System.Console.ReadKey();
                        }
                        linesInDoc = 1;
                    }
                    else
                    {
                        linesInDoc++;
                        bufferDocument.Append(s.Trim() + System.Environment.NewLine);
                    }
                    //save the new document, line by line
                    // newDocument += s + System.Environment.NewLine;
                }
            }

            Console.WriteLine("File add to threadpool!");
            long fileSize = new System.IO.FileInfo(path).Length;
            Console.WriteLine("File size: " + GetBytesReadable(fileSize));
            Console.WriteLine("Total amount:" + Indexer.docNumber + " Documents.");
            Console.WriteLine("Documents in file:" + docNumber + " Documents.");
            Console.WriteLine("-----------------------");

            //  printDic(myPostings);
            //  saveDic(myPostings,"");

            //   System.Console.WriteLine("Press any key to exit.");
            //   System.Console.ReadKey();

            //save doclist

            //DocumentFileToID

            // _pool.Release(4);

            Console.WriteLine("starting " + ReadFileThreads.Count + " threads...");
            foreach (Thread thred in ReadFileThreads)
            {
                thred.Start();
             //   thred.Join();
            }

           
            
            foreach (Thread tread in ReadFileThreads)
            {
                tread.Join();
            }
            

            _myFilePostings.WaitOne();
            Console.WriteLine("Saving File postings on ReadFile-temp-RAM");
            foreach (SortedDictionary<string, string> dic in DicList)
            {
                SortedDictionary<string, string> dic2 = new SortedDictionary<string, string>(dic);
                foreach (KeyValuePair<string, string> entry in dic)
                    if (myFilePostings.ContainsKey(entry.Key))
                        myFilePostings[entry.Key] += " " + entry.Value;
                    else
                        myFilePostings.Add(entry.Key.ToString(), entry.Value);
            }

            Console.WriteLine("postings saved");
            _myFilePostings.ReleaseMutex();




            return myFilePostings;
            //    System.Console.Clear();
        }


        private static void DoWork(ref Semaphore _ReadFileSemaphore, object path, int num, ref List<SortedDictionary<string, string>> DicList)
        {
            string str = path.ToString();
            counter--;
            _ReadFileSemaphore.WaitOne(); //limit threads
         
            SortedDictionary<string, string> newDict = Parse.parseString(str, num);
            //add to main memory first
            //  return newDict;
            DicList.Add(newDict);
            //    ReadFile.saveDic(newDict, postingFilesPath + Interlocked.Increment(ref postingFolderCounter));
            counter++;
            _ReadFileSemaphore.Release();
        }



        /// <summary>
        /// http://stackoverflow.com/questions/7306214/append-lines-to-a-file-using-a-streamwriter
        /// </summary>
        /// <param name="dic">input dictionary.</param>
        /// <param name="directoryPath">output file (full path)</param>
        public static void saveDic(SortedDictionary<string, string> dic, string directoryPath)
        {
          //  var list = dic.Keys.ToList();
           // list.Sort();
           
            char last = ' ';
            string filePath;
            Directory.CreateDirectory(directoryPath);

            ///method: dictionary2file and file2dictionary
            Console.WriteLine("saving " + dic.Count + " terms to HDD payh: " + directoryPath);


            //new StreamWriter(Path, true, Encoding.UTF8, 65536))
            //http://www.jeremyshanks.com/fastest-way-to-write-text-files-to-disk-in-c/
            StreamWriter file2 = new StreamWriter(directoryPath + @"\misc.txt", true, Encoding.UTF8, 65536);
            // Loop through keys.
            foreach (var key in dic)
            {
                string term = key.Key.ToString();

                ///Writing Large Strings
                ///http://www.jeremyshanks.com/fastest-way-to-write-text-files-to-disk-in-c/
                ///

                char c = term[0];
                if (c=='<')
                {
                    string docnumber = term.Split(new char[] { '>', '|' })[1];
                    Indexer._DocumentMetadata.WaitOne();
                    //  Console.WriteLine()
                    // char[] delimiterCharsLang = { '<', '>' };
                    Indexer.DocumentMetadata.Add(docnumber , key.Value);
                   Indexer._DocumentMetadata.ReleaseMutex();
                    continue;

                }
                else
                if (!Char.IsLetter(c))
                {
                    filePath = directoryPath + @"\misc.txt";
                    last = '?';
                }
                else
                {
                    if (last != key.Key.ToString()[0])
                    {
                        file2.Close();
                        filePath = directoryPath + @"\" + key.Key.ToString()[0] + ".txt";
                        file2 = new StreamWriter(filePath, true, Encoding.UTF8, 65536);
                    }
                }

                string line = key.Value;


                //deadlock
                file2.WriteLine(key.Key.ToString() + "^" + line);
                if (Char.IsLetter(key.ToString()[0]))
                {
                    last = key.ToString()[0];
                }
            }


            file2.Close();
        }

        public static SortedDictionary<String, String> fileToDictionary(string path)
        {
            SortedDictionary<string, string> newDic = new SortedDictionary<string, string>();

            using (StreamReader sr = File.OpenText(path))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    //remove blank lines
                    if (s == "")
                        continue;

                    string[] words = s.Split('^');
                    string value = string.Empty;
                    string key = words[0];
                    if (words.Length == 2)
                        value = words[1];
                    if (!newDic.ContainsKey(key))
                        newDic.Add(key, value);
                    else
                    {
                        newDic[key] += " " + value;
                    }

                }
            }
            return newDic;
        }

      

        // Reference 2:
        public static int NaiveSearch(string str, string pattern, int index = 0)
        {

            if (str == null)
                return -1;
            int n = str.Length;
            int m = pattern.Length;
            bool find = true;
            for (int i = index; i < n - m + 1; i++)
            {
                find = true;
                for (int j = 0; j < m; j++)
                {
                    if (str.Substring(i + j, 1) != pattern.Substring(j, 1))
                    {
                        // i++;
                        find = false;
                        break; // jump to next iteration of outer loop
                    }
                }
                if (find)
                    return i;
            }
            return -1;
        }

        //http://www.somacon.com/p576.php
        //http://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net
        // Returns the human-readable file size for an arbitrary, 64-bit file size 
        // The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
        public static string GetBytesReadable(long i)
        {
            // Get absolute value
            long absolute_i = (i < 0 ? -i : i);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);
            // Return formatted number with suffix
            return readable.ToString("0.### ") + suffix;
        }
    }

}
