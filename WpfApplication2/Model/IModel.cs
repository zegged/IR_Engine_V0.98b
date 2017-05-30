﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IR_Engine
{
    interface IModel : INotifyPropertyChanged
    {
        int listBoxMyMovies { set; get; }
        int Progress { set; get; }
        string DocResult { set; get; }
        //    void move(double speed, int angle);
        List<string> autocomplete(string query);


        void selectMovie(string title, double rating);
      

        void createMovieDictionary();
     //   void loadMonths();
        void initiate(); 
   //     void freeMemory(); // create las
   //     void MergeAllToSingleUnSorted();
   //     void sort();
   //     void deleteGarbage();
   //     void loadMetadata();
   //     void UniqueWordsQuery();
      //  void PrintfreqInAllCorpusList(); 
        void mmm();


        

        void ProgressTest();
  //      void dumpDocumentMetadata();
  //      void loadPostingFiles();
  //      void createDictionary();
  //
  //      void loadDictionary();
        void setOutputFolder(string path);
        string getOutputFolder();
    }
}
