/*
  Copyright 2016 TUGraz, http://www.tugraz.at/
  
  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  This project has received funding from the European Union’s Horizon
  2020 research and innovation programme under grant agreement No 644187.
  You may obtain a copy of the License at
  
      http://www.apache.org/licenses/LICENSE-2.0
  
  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
  
  This software has been created in the context of the EU-funded RAGE project.
  Realising and Applied Gaming Eco-System (RAGE), Grant agreement No 644187, 
  http://rageproject.eu/

  Development was done by Cognitive Science Section (CSS) 
  at Knowledge Technologies Institute (KTI)at Graz University of Technology (TUGraz).
  http://kti.tugraz.at/css/

  Created by: Matthias Maurer, TUGraz <mmaurer@tugraz.at>
*/

using AssetManagerPackage;
using AssetPackage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CompetenceComponentNamespace
{
    internal static class CompetenceComponentFunctionality
    {

        #region Fields

        /// <summary>
        /// If true logging is done, otherwise no logging is done.
        /// </summary>
        private static Boolean doLogging = true;

        /// <summary>
        /// Game storage client asset instance
        /// </summary>
        private static GameStorageClientAsset gameStorage;

        /// <summary>
        /// Object dealing with assessment related matters
        /// </summary>
        private static CompetenceAssessmentObject assessmentObject;

        #endregion
        #region TestMethods

        /// <summary>
        /// Diagnostic logging method.
        /// </summary>
        /// 
        /// <param name="msg"> String to be logged.  </param>
        /// <param name="severity"> Severity of the logging-message, optional. </param>
        internal static void loggingCC(String msg, Severity severity = Severity.Information)
        {
            if (doLogging)
                CompetenceComponent.Instance.Log(severity, "[CC]: " + msg);
        }

        #endregion
        #region Methods

        internal static void Initialize()
        {
            //load current assessment state if there is one 
            assessmentObject = new CompetenceAssessmentObject();
        }

        /// <summary>
        /// Method for updating a single competence
        /// </summary>
        /// <param name="competence">string id of the competence for the update</param>
        /// <param name="success">true if the competence is upgraded, false if it is downgraded</param>
        internal static void Update(string competence, bool success)
        {
            throw new NotImplementedException();
        }

        internal static CompetenceComponentSettings getSettings()
        {
            return  (CompetenceComponentSettings)CompetenceComponent.Instance.Settings;
        }

        /// <summary>
        /// Request the string id of the next competence to test/train
        /// </summary>
        /// <returns> the string id of the competence to train/test</returns>
        public static string GetCompetenceRecommendation()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method loading domain model - location specified by settings.
        /// </summary>
        /// <returns>Domain Model for the player.</returns>
        internal static DataModel loadDefaultDataModel()
        {
            loggingCC("Loading default data model.");
            CompetenceComponentSettings ccs = getSettings();
            
            IDataStorage ids = CompetenceComponent.Instance.getInterfaceFromAsset<IDataStorage>();
            if (ids != null)
            {
                if (!ids.Exists(ccs.SourceFile))
                {
                    loggingCC("File " + ccs.SourceFile + " not found for loading data model.", Severity.Error);
                    //throw new Exception("EXCEPTION: File "+ dmas.Source + " not found for loading Domain model.") ;
                    return null;
                }

                loggingCC("Loading data model from File.");
                string xmlDataModel = ids.Load(ccs.SourceFile);
                DataModel dataModel = DataModel.getDMFromXmlString(xmlDataModel);
                return (dataModel);
            }
            else
            {
                loggingCC("IDataStorage bridge absent for requested local loading method of the data model.", Severity.Error);
                //throw new Exception("EXCEPTION: IDataStorage bridge absent for requested local loading method of the Domain model.");
                return null;
            }

        }

        /// <summary>
        /// Method returning the client game storage asset
        /// </summary>
        /// <returns></returns>
        internal static GameStorageClientAsset getGameStorageAsset()
        {
            if (gameStorage == null)
            {
                gameStorage = new GameStorageClientAsset();
                gameStorage.Bridge = AssetManager.Instance.Bridge;
            }
            return gameStorage;
        }


        #endregion
    }

    #region Serialization
    [XmlRoot("datamodel")]
    public class DataModel
    {
        #region Properties

        [XmlElement("elements")]
        public Elements elements { get; set; }

        #endregion
        #region Methods

        public String toXmlString()
        {
            try
            {
                var xmlserializer = new XmlSerializer(typeof(DataModel));
                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlserializer.Serialize(writer, this);
                    String xml = stringWriter.ToString();

                    return (xml);
                }
            }
            catch (Exception ex)
            {
                //throw new Exception("An error occurred", ex);
                CompetenceComponentFunctionality.loggingCC("An error occured while loading data model.");
                return null;
            }
        }

        public void printToCommandline()
        {
            CompetenceComponentFunctionality.loggingCC("Printing data model:");
            CompetenceComponentFunctionality.loggingCC("====================");
            CompetenceComponentFunctionality.loggingCC("Elements:");
            foreach (Competence competence in elements.competenceList)
            {
                CompetenceComponentFunctionality.loggingCC("         -"+competence.id);
            }

        }

        public static DataModel createDummyDataModel(int numberOfCompetences)
        {
            DataModel dm = new DataModel();
            dm.elements = new Elements();
            dm.elements.competenceList = new List<Competence>();
            for (int i=0;i<numberOfCompetences;i++)
            {
                dm.elements.competenceList.Add(new Competence("C"+(i+1).ToString()));
            }
            return dm;
        }

        public static DataModel getDMFromXmlString(String str)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DataModel));
            using (TextReader reader = new StringReader(str))
            {
                DataModel result = (DataModel)serializer.Deserialize(reader);
                return (result);
            }
        }
        #endregion Methods
    }

    public class Elements
    {
        #region Properties

        [XmlElement("competence")]
        public List<Competence> competenceList { get; set; }

        #endregion Properties
    }

    public class Competence
    {
        #region Properties

        [XmlAttribute("id")]
        public string id { get; set; }

        #endregion

        #region Constructor

        public Competence(string id)
        {
            this.id = id;
        }

        public Competence() { }

        #endregion
    }

    #endregion
    #region Assessment

    internal class CompetenceAssessmentObject
    {
        #region Fields 
        public List<AssessmentCompetence> competences;
        #endregion
        #region Constructor
        public CompetenceAssessmentObject()
        {
            DataModel dataModel = CompetenceComponentFunctionality.loadDefaultDataModel();
            competences = new List<AssessmentCompetence>();
            CompetenceComponentSettings settings = CompetenceComponentFunctionality.getSettings();
            float initialValue = (1.0f/(float)settings.NumberOfLevels)/2.0f;

            foreach (Competence competence in dataModel.elements.competenceList)
            {
                competences.Add(new AssessmentCompetence(competence.id, initialValue));
            }
        }
        #endregion
        #region Methods 

        public AssessmentCompetence getAssessmentCompetenceById(string id)
        {
            foreach (AssessmentCompetence competence in competences)
                if (competence.id.Equals(id))
                    return competence;
            return null;
        }

        public void loadAssessmentState()
        {
            GameStorageClientAsset gameStorage = CompetenceComponentFunctionality.getGameStorageAsset();
            StorageLocations storageLocation = StorageLocations.Local;

            //try to load model, if possible -> load assessment state, else create model and store model + competence state
            String model = "CompetenceComponent_AssessmentState";

            gameStorage.AddModel(model);
            Boolean isStructureRestored = gameStorage.LoadStructure(model, storageLocation, SerializingFormat.Xml);
            if (isStructureRestored)
            {
                CompetenceComponentFunctionality.loggingCC("Assessment state structure was restored from local file.");

            
                gameStorage.LoadData(model, StorageLocations.Local, SerializingFormat.Xml);
                foreach (Node node in gameStorage[model].Children)
                {
                    string loadedValue = (string)node.Value;
                    string id = node.Name;
                    string[] competenceValues = loadedValue.Split('&');
                    float value = float.Parse(competenceValues[0]);
                    DateTime timeStamp = DateTime.Parse(competenceValues[1]);

                    AssessmentCompetence competence = getAssessmentCompetenceById(node.Name);
                    competence.timestamp = timeStamp;
                    //change value after loading here
                    competence.value = value;
                }


                CompetenceComponentFunctionality.loggingCC("Competence values restored from local file.");
            }
            else
            {
                CompetenceComponentFunctionality.loggingCC("Assessment state structure could not be restored from local file - creating new one.");
                foreach (AssessmentCompetence comp in competences)
                    gameStorage[model].AddChild(comp.id, storageLocation).Value = comp.value.ToString()+"&"+comp.timestamp;

                gameStorage.SaveStructure(model, storageLocation, SerializingFormat.Xml);
                gameStorage.SaveData(model, storageLocation, SerializingFormat.Xml);
            }
        }

        public void storeAssessmentState()
        {
            GameStorageClientAsset gameStorage = CompetenceComponentFunctionality.getGameStorageAsset();
            StorageLocations storageLocation = StorageLocations.Local;
            String model = "CompetenceComponent_AssessmentState";

            //gameStorage.AddModel(model);

            foreach (AssessmentCompetence comp in competences)
                gameStorage[model].AddChild(comp.id, storageLocation).Value = comp.value.ToString() + "&" + comp.timestamp;

            //gameStorage.SaveStructure(model, storageLocation, SerializingFormat.Xml);
            gameStorage.SaveData(model, storageLocation, SerializingFormat.Xml);
        }

        #endregion
    }

    internal class AssessmentCompetence
    {
        #region Fields
        public string id;
        public DateTime timestamp;
        public float value;
        #endregion
        #region Methods
        public void setTimestamp()
        {
            timestamp = DateTime.Now;
        }
        #endregion
        #region Constructors
        public AssessmentCompetence(string id, float value)
        {
            this.id = id;
            this.timestamp = DateTime.Now;
            this.value = value;
        }
        #endregion
    }

    #endregion
}
