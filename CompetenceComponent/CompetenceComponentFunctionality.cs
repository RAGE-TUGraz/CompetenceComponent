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
            assessmentObject.loadAssessmentState();

            assessmentObject.updateDueToTimeGap();
        }

        /// <summary>
        /// Method for updating a single competence
        /// </summary>
        /// <param name="competence">string id of the competence for the update</param>
        /// <param name="success">true if the competence is upgraded, false if it is downgraded</param>
        internal static void UpdateCompetence(string competence, bool success)
        {
            assessmentObject.updateCompetence(competence,success);
        }

        internal static void UpdateGamesituation(string gamesituation, bool success)
        {
            assessmentObject.updateGamesituation(gamesituation,success);
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
            AssessmentCompetence nextCompetence = CompetenceRecommendationObject.getCompetenceRecommendation(assessmentObject.competences);
            return nextCompetence.id;
        }

        public static string GetGamesituationRecommendation()
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
                    throw new DataModelNotFoundException("EXCEPTION: File "+ ccs.SourceFile + " not found for loading Domain model.") ;
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

        internal static CompetenceAssessmentObject getAssessmentObject()
        {
            return assessmentObject;
        }

        internal static void resetCompetenceState()
        {
            assessmentObject.resetCompetenceState();
        }

        public static Dictionary<string,int> getCompetencelevels()
        {
            return assessmentObject.getCompetenceLevels();
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
            CompetenceComponentFunctionality.loggingCC("Competencies:");
            foreach (Competence competence in elements.competenceList.competences)
            {
                CompetenceComponentFunctionality.loggingCC("             -" + competence.id);
            }
            CompetenceComponentFunctionality.loggingCC("Gamesituations:");
            foreach (Gamesituation situation in elements.gamesituationList.gamesituations)
            {
                CompetenceComponentFunctionality.loggingCC("             -" + situation.id+"("+situation.difficulty+")");
                foreach (GamesituationCompetence competence in situation.competences)
                {
                    CompetenceComponentFunctionality.loggingCC("                 -" + competence.id +"("+competence.weight+")");
                }
            }

        }

        public static DataModel createDummyDataModel(int numberOfCompetences)
        {
            DataModel dm = new DataModel();
            dm.elements = new Elements();
            dm.elements.competenceList.competences = new List<Competence>();
            for (int i=0;i<numberOfCompetences;i++)
            {
                dm.elements.competenceList.competences.Add(new Competence("C"+(i+1).ToString()));
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


        [XmlElement("competences")]
        public CompetenceList competenceList { get; set; }

        [XmlElement("gamesituations")]
        public GamesituationList gamesituationList { get; set; }

        #endregion Properties
    }

    public class CompetenceList
    {
        #region Properties

        [XmlElement("competence")]
        public List<Competence> competences { get; set; }

        #endregion
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

    public class GamesituationList
    {
        #region Properties

        [XmlElement("gamesituation")]
        public List<Gamesituation> gamesituations { get; set; }
        
        #endregion
    }

    public class Gamesituation
    {
        #region Properties

        [XmlAttribute("id")]
        public string id { get; set; }


        [XmlAttribute("difficulty")]
        public float difficulty { get; set; }


        [XmlElement("competence")]
        public List<GamesituationCompetence> competences { get; set; }

        #endregion
    }

    public class GamesituationCompetence
    {
        #region Properties

        [XmlAttribute("id")]
        public string id { get; set; }

        [XmlAttribute("weight")]
        public float weight { get; set; }

        #endregion
    }

    #endregion
    #region Assessment

    public class CompetenceAssessmentObject
    {
        #region Fields 
        public List<AssessmentCompetence> competences;
        public List<AssessmentGamesituation> gamesituations;
        #endregion
        #region Constructor
        public CompetenceAssessmentObject()
        {
            createInitialValues();
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

        public AssessmentGamesituation getAssessmentGamesituationById(string id)
        {
            foreach (AssessmentGamesituation gamesituation in gamesituations)
                if (gamesituation.id.Equals(id))
                    return gamesituation;
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

        public void updateCompetence(string competenceId, bool success, float factor=1.0f)
        {
            AssessmentCompetence ac = getAssessmentCompetenceById(competenceId);
            if (ac == null)
            {
                CompetenceComponentFunctionality.loggingCC("Cannot update competence '"+competenceId+"' - not existent in data model.");
                return;
            }
            CompetenceComponentFunctionality.loggingCC("Update competence '" + competenceId + "' with factor "+factor);

            float updateValue = 1.0f / (float) CompetenceComponentFunctionality.getSettings().NumberOfLevels;
            updateValue *= factor;
            ac.value = success ? ac.value + updateValue : ac.value - updateValue;
            ac.value = Math.Max(Math.Min(1, ac.value), 0);
            ac.setTimestamp();
            storeAssessmentState();
        }

        public void updateGamesituation(string gamesituationId, bool success)
        {
            AssessmentGamesituation situation = getAssessmentGamesituationById(gamesituationId);
            if (situation == null)
            {
                CompetenceComponentFunctionality.loggingCC("Can't update: GS '"+gamesituationId+"' not found");
                return;
            }

            CompetenceComponentFunctionality.loggingCC("Update according to GS '" + gamesituationId + "'");
            foreach (AssessmentGamesituationCompetence competence in situation.competences)
            {
                updateCompetence(competence.id,success,situation.difficulty*competence.weight);
            }
        }

        public void resetCompetenceState()
        {
            createInitialValues();
            storeAssessmentState();
        }

        public void createInitialValues()
        {
            DataModel dataModel = CompetenceComponentFunctionality.loadDefaultDataModel();
            competences = new List<AssessmentCompetence>();
            CompetenceComponentSettings settings = CompetenceComponentFunctionality.getSettings();
            float initialValue = (1.0f / (float)settings.NumberOfLevels) / 2.0f;

            foreach (Competence competence in dataModel.elements.competenceList.competences)
            {
                competences.Add(new AssessmentCompetence(competence.id, initialValue));
            }

            gamesituations = new List<AssessmentGamesituation>();
            foreach (Gamesituation situation in dataModel.elements.gamesituationList.gamesituations)
            {
                gamesituations.Add(new AssessmentGamesituation(situation));
            }
        }

        public void updateDueToTimeGap()
        {
            float linearDecreasion = CompetenceComponentFunctionality.getSettings().LinearDecreasionOfCompetenceValuePerDay;
            foreach (AssessmentCompetence competence in competences)
            {
                TimeSpan deltaTime = competence.timestamp - DateTime.Now;
                competence.value -= (float)deltaTime.TotalDays * linearDecreasion;
                competence.value = Math.Max(competence.value,0f);
                competence.setTimestamp();
            }
        }

        public Dictionary<string,int> getCompetenceLevels()
        {
            //assign levels to competences according to number of levels
            float levelWidth = 1.0f / (float)CompetenceComponentFunctionality.getSettings().NumberOfLevels;
            Dictionary<string, int> competenceLevels = new Dictionary<string, int>();
            foreach (AssessmentCompetence competence in competences)
            {
                int level = (int)Math.Floor(competence.value / levelWidth);
                level = Math.Min(level, CompetenceComponentFunctionality.getSettings().NumberOfLevels-1);
                competenceLevels[competence.id] = level;
            }

            return competenceLevels;
        }

        #endregion
        #region Testmethods

        public void printToConsole()
        {
            CompetenceComponentFunctionality.loggingCC("Printing evaluation state:");
            CompetenceComponentFunctionality.loggingCC("==========================");
            CompetenceComponentFunctionality.loggingCC("Elements:");
            foreach (AssessmentCompetence competence in competences)
            {
                CompetenceComponentFunctionality.loggingCC("         -" + competence.id + "::"+Math.Round(competence.value,2)+"::"+competence.timestamp.ToString());
            }

        }

        #endregion
    }

    public class AssessmentCompetence
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

    public class AssessmentGamesituation
    {
        #region Fields
        public string id;
        public float difficulty;
        public List<AssessmentGamesituationCompetence> competences = new List<AssessmentGamesituationCompetence>();
        #endregion
        #region Constructors
        public AssessmentGamesituation(Gamesituation gamesituation)
        {
            id = gamesituation.id;
            difficulty = gamesituation.difficulty;
            foreach (GamesituationCompetence competence in gamesituation.competences)
            {
                competences.Add(new AssessmentGamesituationCompetence(competence));
            }
        }
        #endregion
    }

    public class AssessmentGamesituationCompetence
    {
        #region Fields
        public string id;
        public float weight;
        #endregion
        #region Constructors
        public AssessmentGamesituationCompetence(GamesituationCompetence competence)
        {
            id = competence.id;
            weight = competence.weight;
        }
        #endregion
    }

    #endregion
    #region Recommendation

    public class CompetenceRecommendationObject
    {
        #region Fields
        #endregion
        #region Methods

        public static AssessmentCompetence getCompetenceRecommendation(List<AssessmentCompetence> competences)
        { 
            //assign levels to competences according to number of levels
            float levelWidth = 1.0f/(float)CompetenceComponentFunctionality.getSettings().NumberOfLevels;
            Dictionary<AssessmentCompetence, int> competenceLevels = new Dictionary<AssessmentCompetence, int>();
            foreach (AssessmentCompetence competence in competences)
            {
                int level = (int) Math.Floor(competence.value / levelWidth);
                competenceLevels[competence] = level;
            }

            //get competences with lowest levels
            List<AssessmentCompetence> minLevelCompetences = null;
            int minLevel = int.MaxValue;
            foreach (AssessmentCompetence competence in competences)
            {
                int competenceLevel = competenceLevels[competence];
                if (competenceLevel < minLevel)
                {
                    minLevelCompetences = new List<AssessmentCompetence>();
                    minLevel = competenceLevel;
                }
                if (minLevel == competenceLevel)
                {
                    minLevelCompetences.Add(competence);
                }
            }

            //get competence with oldest timestamp
            List<AssessmentCompetence> minLevelOldestTimestampCompetence = new List<AssessmentCompetence>();
            DateTime oldestTimeStamp = DateTime.Now;
            foreach (AssessmentCompetence competence in minLevelCompetences)
            {
                DateTime dateTime = competence.timestamp;
                if (dateTime.CompareTo(oldestTimeStamp)<0)
                {
                    minLevelOldestTimestampCompetence = new List<AssessmentCompetence>();
                    oldestTimeStamp = dateTime;
                }
                if (oldestTimeStamp.CompareTo(dateTime)==0)
                {
                    minLevelOldestTimestampCompetence.Add(competence);
                }
            }

            //select random competence from min level oldest timestamp competences
            Random rnd = new Random();
            int r = rnd.Next(minLevelOldestTimestampCompetence.Count);
            AssessmentCompetence selectedCompetence = minLevelOldestTimestampCompetence[r];

            return selectedCompetence;
        }

        #endregion
    }

    #endregion
    #region Exceptions
    public class DataModelNotFoundException : Exception
    {
        public DataModelNotFoundException()
        {
        }

        public DataModelNotFoundException(string message)
            : base(message)
        {
        }

        public DataModelNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    #endregion
}
