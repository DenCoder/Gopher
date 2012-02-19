﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Gopher.Core.Logging;
using Gopher.Core.Models;
using Newtonsoft.Json;

namespace Gopher.Core.Data.Json.Repositories {
  [Export(typeof(IFolderRepository))]
  public class JsonFolderRepository : IFolderRepository {
    private readonly ILogger logger;
    private readonly string pathToJsonFile;
    int folderIndex = 1;

    public JsonFolderRepository(string pathToJsonFile, ILogger logger, bool append = false) {
      this.pathToJsonFile = pathToJsonFile;
      this.logger = logger;

      if (!System.IO.File.Exists(pathToJsonFile)) {
        System.IO.File.CreateText(pathToJsonFile).Close();
      } else {
        if (append) {
          var folders = GetFolders();
          if (folders.Count() != 0) {
            folderIndex = folders.OrderByDescending(p => p.FolderId).First().FolderId + 1;
          }
          //throw new Exception("The project folder database already exists");
        }
      }
    }

    #region IFolderRepository Members

    public Folder GetById(int folderId) {      
      throw new NotImplementedException("This method is not implemented in the JsonFolderRepository for performance reasons.");
    }
    
    public IEnumerable<Folder> GetFolders() {
      var folders = new List<Folder>();
      using (var stream = new System.IO.StreamReader(pathToJsonFile)) {
        while (!stream.EndOfStream) {
          string folderJson = string.Empty;
          try {
            folderJson = stream.ReadLine();
            Folder folder = JsonConvert.DeserializeObject<Folder>(folderJson);
            folders.Add(folder);
          } catch (Exception ex) {
            logger.ErrorException("Error deserializing Folder from json: " + folderJson, ex);
          }
          
        }
        stream.Close();
      }
      return folders;
    }

    public Folder Add(Folder folderToAdd) {
      folderToAdd.FolderId = folderIndex++;
      string folderJson = JsonConvert.SerializeObject(folderToAdd);
      using (var stream = new System.IO.StreamWriter(pathToJsonFile, append: true)) {
        stream.WriteLine(folderJson);
        stream.Close();
      }
      //Folders.Add(folderToAdd);
      return folderToAdd;
    }

    #endregion
  }
}