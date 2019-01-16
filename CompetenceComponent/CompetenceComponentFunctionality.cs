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
        internal static CompetenceAssessmentObject assessmentObject;

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
        }

        internal static void updateDueToForgetting()
        {
            assessmentObject.updateDueToForgetting();
        }

        /// <summary>
        /// Method for updating a single competence
        /// </summary>
        /// <param name="competence">string id of the competence for the update</param>
        /// <param name="success">true if the competence is upgraded, false if it is downgraded</param>
        internal static void UpdateCompetence(string competence, bool success, UpdateType type)
        {
            assessmentObject.updateCompetence(competence, success, type);
        }

        internal static Dictionary<string, float[]> getCompetenceValues()
        {
            return assessmentObject.getCompetenceValues();
        }

        internal static void setCompetenceValues(string competenceId, float learningValue, float assessmentValue)
        {
            assessmentObject.setCompetenceValues(competenceId, learningValue, assessmentValue);
        }

        internal static GamesituationUpdateDescription UpdateGamesituation(string gamesituation, bool success, float quality)
        {
            return assessmentObject.updateGamesituation(gamesituation, success, quality);
        }

        internal static CompetenceComponentSettings getSettings()
        {
            return (CompetenceComponentSettings)CompetenceComponent.Instance.Settings;
        }

        /// <summary>
        /// Request the string id of the next competence to test/train
        /// </summary>
        /// <returns> the string id of the competence to train/test</returns>
        public static string GetCompetenceRecommendation(UpdateType type)
        {
            AssessmentCompetence nextCompetence = RecommendationObject.getCompetenceRecommendation(assessmentObject.competences, type);
            if (nextCompetence == null)
                return null;
            return nextCompetence.id;
        }

        public static List<string> GetGamesituationRecommendation(int quantity)
        {
            List<AssessmentGamesituation> nextGamesituations = RecommendationObject.getGamesituationRecommendation(assessmentObject.competences, assessmentObject.gamesituations);

            List<string> nextGSs = new List<string>();
            foreach (AssessmentGamesituation gs in nextGamesituations)
            {
                if (nextGSs.Count==quantity)
                {
                    break;
                }else if(gs != null)
                {
                    nextGSs.Add(gs.id);
                }
            }
            
            return nextGSs;
        }

        internal static void storeDataModel(DataModel dataModel, string filepath)
        {
            loggingCC("Storing data model: "+filepath);
            CompetenceComponentSettings ccs = getSettings();
            string dataModelString = dataModel.toXmlString();

            IDataStorage ids = CompetenceComponent.Instance.getInterfaceFromAsset<IDataStorage>();
            if (ids != null)
            {
                ids.Save(filepath, dataModelString);
                loggingCC("Storing data model - done ");
            }
            else
            {
                loggingCC("IDataStorage bridge absent for requested local storing method of the data model.", Severity.Error);
                //throw new Exception("EXCEPTION: IDataStorage bridge absent for requested local loading method of the Domain model.");
            }
        }

        /// <summary>
        /// Method loading domain model - location specified by settings.
        /// </summary>
        /// <returns>Domain Model for the player.</returns>
        internal static DataModel loadDefaultDataModel()
        {
            CompetenceComponentSettings ccs = getSettings();
            return loadDataModel(ccs.SourceFile);
        }

        internal static DataModel loadDataModel(string filepath)
        {
            loggingCC("Loading data model.");
            
            IDataStorage ids = CompetenceComponent.Instance.getInterfaceFromAsset<IDataStorage>();

            if (ids != null)
            {
                if (!ids.Exists(filepath))
                {
                    loggingCC("File " + filepath + " not found for loading data model.", Severity.Error);
                    throw new DataModelNotFoundException("EXCEPTION: File " + filepath + " not found for loading Data model.");
                }

                loggingCC("Loading data model from File.");
                string xmlDataModel = ids.Load(filepath);
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

        public static Dictionary<string, int[]> getCompetencelevels()
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

        [XmlElement("relations")]
        public Relations relations { get; set; }

        [XmlElement("mappings")]
        public Mappings mappings { get; set; }

        #endregion
        #region API
        public void storeToFile(string filePath)
        {
            CompetenceComponentFunctionality.storeDataModel(this, filePath);
        }

        public static DataModel loadFromFile(string filePath)
        {
            return CompetenceComponentFunctionality.loadDataModel(filePath);
        }
        
        public void addCompetence(string id)
        {
            if (getElementCompetenceById(id) == null)
            {
                elements.competenceList.competences.Add(new Competence(id));
            }
            else
            {
                CompetenceComponentFunctionality.loggingCC("Can't add Competence '" + id + "', this competence already exists.");
            }
        }

        public void addPrerequisites(string id, List<string> prerequisites)
        {
            //check - are competences available?
            if (getElementCompetenceById(id) == null)
            {
                CompetenceComponentFunctionality.loggingCC("Can't add prerequisites to competence '" + id + "' - it doesn't exist.");
                return;
            }
            foreach (string prereq_id in prerequisites)
            {
                if (getElementCompetenceById(prereq_id) == null)
                {
                    CompetenceComponentFunctionality.loggingCC("Can't add prerequisites '" + prereq_id + "' - it doesn't exist.");
                    return;
                }
            }
            //check for created loops
            ///---------------
            //add prerequisites - Relation
            PrerequisiteCompetence prerequisiteCompetence = getRelationPrerequisiteById(id);
            if (prerequisiteCompetence == null)
            {
                prerequisiteCompetence = new PrerequisiteCompetence();
                prerequisiteCompetence.id = id;
                relations.competenceprerequisites.competenceList.Add(prerequisiteCompetence);
            }
            //add prerequisites
            foreach (string prereq_id in prerequisites)
            {
                if (!prerequisiteCompetence.containsPrerequisiteById(prereq_id))
                {
                    prerequisiteCompetence.prerequisites.Add(new Prerequisite(prereq_id));
                }
            }
        }

        public void addGamesituation(string id, string difficulty, bool isLearning, bool isAssessment, Dictionary<string,float> competencies)
        {
            //does gamesituation id already exists?
            if (getElementGamesituationById(id) != null)
            {
                CompetenceComponentFunctionality.loggingCC("Can't add Gamesituation '"+id+"', this gamesituation already exists.");
                return;
            }
            //does difficulty exists?
            if (getDifficultyById(difficulty)==null)
            {
                CompetenceComponentFunctionality.loggingCC("Can't add Gamesituation, because difficulty '" + difficulty + "' unknown.");
                return;
            }
            //create object
            float sumOfWeights = 0;
            Gamesituation gamesituation = new Gamesituation();
            gamesituation.id = id;
            gamesituation.difficulty = difficulty;
            foreach (string comp_id in competencies.Keys)
            {
                if (getElementCompetenceById(comp_id)==null)
                {
                    CompetenceComponentFunctionality.loggingCC("Can't add Gamesituation - competence '" + comp_id + "' doesn't exist.");
                    return;
                }
                if (competencies[comp_id] <= 0)
                {
                    CompetenceComponentFunctionality.loggingCC("Can't add Gamesituation - competence weights need to be > 0.");
                    return;
                }
                sumOfWeights += competencies[comp_id];
                GamesituationCompetence competence = new GamesituationCompetence();
                competence.id = comp_id;
                competence.weight = competencies[comp_id];
                gamesituation.competences.Add(competence);
            }
            //adjust weights - they need to equal 1 when summed up
            if (sumOfWeights != 1)
            {
                foreach (GamesituationCompetence competence in gamesituation.competences)
                {
                    competence.weight /= sumOfWeights;
                }
            }
            //add gamesituation
            elements.gamesituationList.gamesituations.Add(gamesituation);
        }

        public void addDifficulty(string id, float weight)
        {
            if (getDifficultyById(id) != null)
            {
                CompetenceComponentFunctionality.loggingCC("Can't add Difficulty '" + id + "' - this id already exists.");
                return;
            }
            if (weight <=0)
            {
                CompetenceComponentFunctionality.loggingCC("Can't add Difficulty '" + id + "' - weight needs to be >0.");
                return;
            }
            Difficulty difficulty = new Difficulty();
            difficulty.id = id;
            difficulty.weight = weight;

            mappings.difficulties.difficultyList.Add(difficulty);
        }

        #endregion
        #region InternalMethods
        private PrerequisiteCompetence getRelationPrerequisiteById(string id)
        {
            foreach (PrerequisiteCompetence prerequisite in relations.competenceprerequisites.competenceList)
            {
                if (prerequisite.id.Equals(id))
                {
                    return prerequisite;
                }
            }
            return null;
        }

        private Competence getElementCompetenceById(string id)
        {
            foreach (Competence competence in elements.competenceList.competences)
            {
                if (competence.id.Equals(id))
                {
                    return competence;
                }
            }
            return null;
        }

        private Gamesituation getElementGamesituationById(string id)
        {
            foreach (Gamesituation gamesituation in elements.gamesituationList.gamesituations)
            {
                if (gamesituation.id.Equals(id))
                {
                    return gamesituation;
                }
            }
            return null;
        }

        private Difficulty getDifficultyById(string id)
        {
            foreach (Difficulty difficulty in mappings.difficulties.difficultyList)
            {
                if (difficulty.id.Equals(id))
                {
                    return difficulty;
                }
            }
            return null;
        }
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
            CompetenceComponentFunctionality.loggingCC("Prerequisites:");
            foreach (PrerequisiteCompetence competence in relations.competenceprerequisites.competenceList)
            {
                string text = "             -" + competence.id + ": ";
                foreach (Prerequisite prerequisite in competence.prerequisites)
                {
                    text += prerequisite.id + ", ";
                }
                CompetenceComponentFunctionality.loggingCC(text);
            }
            CompetenceComponentFunctionality.loggingCC("Gamesituations:");
            foreach (Gamesituation situation in elements.gamesituationList.gamesituations)
            {
                CompetenceComponentFunctionality.loggingCC("             -" + situation.id + "(D:" + situation.difficulty + ",L:" + situation.isLearning + ",A:" + situation.isAssessment + ")");
                foreach (GamesituationCompetence competence in situation.competences)
                {
                    CompetenceComponentFunctionality.loggingCC("                 -" + competence.id + "(" + competence.weight + ")");
                }
            }
            CompetenceComponentFunctionality.loggingCC("Difficulties:");
            foreach (Difficulty difficulty in mappings.difficulties.difficultyList)
            {
                CompetenceComponentFunctionality.loggingCC("             -" + difficulty.id + ":" + difficulty.weight);
            }

        }

        public static DataModel createDummyDataModel(int numberOfCompetences)
        {
            DataModel dm = new DataModel();
            dm.elements = new Elements();
            dm.elements.competenceList.competences = new List<Competence>();
            for (int i = 0; i < numberOfCompetences; i++)
            {
                dm.elements.competenceList.competences.Add(new Competence("C" + (i + 1).ToString()));
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
        #region Constructor
        public DataModel()
        {
            elements = new Elements();
            relations = new Relations();
            mappings = new Mappings();
        }
        #endregion
    }

    public class Relations
    {
        #region Properties

        public Competenceprerequisites competenceprerequisites;
        #endregion
        #region Constructor
        public Relations()
        {
            competenceprerequisites = new Competenceprerequisites();
        }
        #endregion
    }

    public class Competenceprerequisites
    {
        #region Properties
        [XmlElement("competence")]
        public List<PrerequisiteCompetence> competenceList { get; set; }
        #endregion
        #region Constructor
        public Competenceprerequisites()
        {
            competenceList = new List<PrerequisiteCompetence>();
        }
        #endregion
    }

    public class PrerequisiteCompetence
    {
        #region Properties
        [XmlAttribute("id")]
        public string id;

        [XmlElement("prereqcompetence")]
        public List<Prerequisite> prerequisites;
        #endregion
        #region Constructor
        public PrerequisiteCompetence()
        {
            prerequisites = new List<Prerequisite>();
        }
        #endregion
        #region Methods
        public bool containsPrerequisiteById(string id)
        {
            foreach (Prerequisite prereq in prerequisites)
            {
                if (prereq.id.Equals(id))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }

    public class Prerequisite
    {
        #region Properties

        [XmlAttribute("id")]
        public string id;
        #endregion
        #region Constructor
        public Prerequisite() { }

        public Prerequisite(string id)
        {
            this.id = id;
        }
        #endregion
    }

    public class Mappings
    {
        #region Properties
        [XmlElement("difficulties")]
        public Difficulties difficulties { get; set; }
        #endregion
        #region Constructor
        public Mappings()
        {
            difficulties = new Difficulties();
        }
        #endregion
    }

    public class Difficulties
    {
        #region Properties
        [XmlElement("difficulty")]
        public List<Difficulty> difficultyList { get; set; }
        #endregion
        #region Methods

        public float getDifficulty(string id)
        {
            foreach (Difficulty difficulty in difficultyList)
            {
                if (id.Equals(difficulty.id))
                {
                    return difficulty.weight;
                }
            }
            return -1;
        }
        #endregion
        #region Constructor
        public Difficulties()
        {
            difficultyList = new List<Difficulty>();
        }
        #endregion
    }

    public class Difficulty
    {
        #region Properties
        [XmlAttribute("id")]
        public string id { get; set; }

        [XmlAttribute("weight")]
        public float weight { get; set; }

        #endregion
    }

    public class Elements
    {
        #region Properties


        [XmlElement("competences")]
        public CompetenceList competenceList { get; set; }

        [XmlElement("gamesituations")]
        public GamesituationList gamesituationList { get; set; }

        #endregion Properties
        #region Constructor
        public Elements()
        {
            competenceList = new CompetenceList();
            gamesituationList = new GamesituationList();
        }
        #endregion
    }

    public class CompetenceList
    {
        #region Properties

        [XmlElement("competence")]
        public List<Competence> competences { get; set; }

        #endregion
        #region Constructor
        public CompetenceList()
        {
            competences = new List<Competence>();
        }
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
        #region Constructor
        public GamesituationList()
        {
            gamesituations = new List<Gamesituation>();
        }
        #endregion
    }

    public class Gamesituation
    {
        #region Properties

        [XmlAttribute("id")]
        public string id { get; set; }


        [XmlAttribute("difficulty")]
        public string difficulty { get; set; }


        [XmlAttribute("assessment")]
        public bool isAssessment { get; set; }


        [XmlAttribute("learning")]
        public bool isLearning { get; set; }

        [XmlElement("competence")]
        public List<GamesituationCompetence> competences { get; set; }

        #endregion
        #region Constructor
        public Gamesituation()
        {
            competences = new List<GamesituationCompetence>();
        }
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

    public enum UpdateType { ASSESSMENT, LEARNING };

    public enum Timestamp { ASSESSMENT, LEARNING, FORGETTING };

    public class CompetenceAssessmentObject
    {
        #region Fields 
        public List<AssessmentCompetence> competences;
        public List<AssessmentGamesituation> gamesituations;
        public Difficulties difficulties;
        private string dataModelName = null;
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

        private string getStorageModelName()
        {
            if (dataModelName == null)
            {
                dataModelName= CompetenceComponentFunctionality.getSettings().CompetenceValueStoragePrefix;
            }
            
            return dataModelName+"_CompetenceComponent_AssessmentState";
        }

        public void loadAssessmentState()
        {
            GameStorageClientAsset gameStorage = CompetenceComponentFunctionality.getGameStorageAsset();
            StorageLocations storageLocation = StorageLocations.Local;

            //try to load model, if possible -> load assessment state, else create model and store model + competence state
            String model = getStorageModelName();

            try
            {
                gameStorage.AddModel(model);
            }
            catch (Exception e)
            {

            }
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
                    float valueAssessment = float.Parse(competenceValues[0]);
                    float valueLearning = float.Parse(competenceValues[1]);
                    DateTime timeStamp1 = DateTime.Parse(competenceValues[2]);
                    DateTime timeStamp2 = DateTime.Parse(competenceValues[3]);
                    DateTime timeStamp3 = DateTime.Parse(competenceValues[4]);

                    AssessmentCompetence competence = getAssessmentCompetenceById(node.Name);
                    competence.timestampAssessment = timeStamp1;
                    competence.timestampLearning = timeStamp2;
                    competence.timestampForgetting = timeStamp3;
                    //change value after loading here
                    competence.valueAssessment = valueAssessment;
                    competence.valueLearning = valueLearning;
                }


                CompetenceComponentFunctionality.loggingCC("Competence values restored from local file.");
            }
            else
            {
                CompetenceComponentFunctionality.loggingCC("Assessment state structure could not be restored from local file - creating new one.");
                foreach (AssessmentCompetence comp in competences)
                    gameStorage[model].AddChild(comp.id, storageLocation).Value = comp.valueAssessment.ToString() + "&" + comp.valueLearning.ToString()
                        + "&" + comp.timestampAssessment + "&" + comp.timestampLearning + "&" + comp.timestampForgetting;

                gameStorage.SaveStructure(model, storageLocation, SerializingFormat.Xml);
                gameStorage.SaveData(model, storageLocation, SerializingFormat.Xml);
            }
        }

        public void storeAssessmentState()
        {
            GameStorageClientAsset gameStorage = CompetenceComponentFunctionality.getGameStorageAsset();
            StorageLocations storageLocation = StorageLocations.Local;
            String model = getStorageModelName();

            //gameStorage.AddModel(model);

            foreach (AssessmentCompetence comp in competences)
                gameStorage[model].AddChild(comp.id, storageLocation).Value = comp.valueAssessment.ToString() + "&" + comp.valueLearning.ToString()
                    + "&" + comp.timestampAssessment + "&" + comp.timestampLearning + "&" + comp.timestampForgetting;

            //gameStorage.SaveStructure(model, storageLocation, SerializingFormat.Xml);
            gameStorage.SaveData(model, storageLocation, SerializingFormat.Xml);
        }

        public void updateCompetence(string competenceId, bool success, UpdateType type, float quality, float factor = 1.0f)
        {
            AssessmentCompetence ac = getAssessmentCompetenceById(competenceId);
            if (ac == null)
            {
                CompetenceComponentFunctionality.loggingCC("Cannot update competence '" + competenceId + "' - not existent in data model.");
                return;
            }
            CompetenceComponentFunctionality.loggingCC("Update competence '" + competenceId + "' with type '" + ((type == UpdateType.ASSESSMENT) ? "Assessment" : "Learning") + "' and with factor " + factor);

            float updateValue = 1.0f / (float)CompetenceComponentFunctionality.getSettings().NumberOfLevels;
            updateValue *= factor;
            updateValue *= quality;
            float oldValue=0;
            float newValue=0;
            if (type.Equals(UpdateType.ASSESSMENT))
            {
                oldValue = ac.valueAssessment;
                ac.valueAssessment = success ? ac.valueAssessment + updateValue : ac.valueAssessment - updateValue;
                ac.valueAssessment = Math.Max(Math.Min(1, ac.valueAssessment), 0);
                ac.valueLearning = Math.Max(ac.valueAssessment, ac.valueLearning);
                newValue = ac.valueAssessment;
            }
            else if (type.Equals(UpdateType.LEARNING))
            {
                oldValue = ac.valueLearning;
                ac.valueLearning = success ? ac.valueLearning + updateValue : ac.valueLearning - updateValue;
                ac.valueLearning = Math.Max(Math.Min(1, ac.valueLearning), 0);
                //learning value of a competence can not rise over prerequisite value
                ac.valueLearning = Math.Min(ac.valueLearning, ac.getMinimumPrerequisiteValueOrOneIfThereAreNoPrerequisites(UpdateType.LEARNING));
                newValue = ac.valueLearning;
            }
            CompetenceComponentFunctionality.loggingCC("Updated Competence ("+ (type.Equals(UpdateType.LEARNING) ? "Learning" : "Assessment") +") '"+ac.id+"' from "+oldValue+" to "+newValue);
            Timestamp stamp = type.Equals(UpdateType.ASSESSMENT) ? Timestamp.ASSESSMENT : Timestamp.LEARNING;
            ac.setTimestamp(stamp);
            storeAssessmentState();
        }

        public GamesituationUpdateDescription updateGamesituation(string gamesituationId, bool success, float quality)
        {
            AssessmentGamesituation situation = getAssessmentGamesituationById(gamesituationId);
            if (situation == null)
            {
                CompetenceComponentFunctionality.loggingCC("Can't update: GS '" + gamesituationId + "' not found");
                return null;
            }

            CompetenceComponentFunctionality.loggingCC("Update according to GS '" + gamesituationId + "'");
            GamesituationUpdateDescription updateInfoGs = new GamesituationUpdateDescription();
            updateInfoGs.gamesituationId = gamesituationId;
            foreach (AssessmentGamesituationCompetence competence in situation.competences)
            {
                //set update infos 1
                CompetenceUpdateDescription newCompetenceUpdateInfo = new CompetenceUpdateDescription();
                AssessmentCompetence aCompetence = getAssessmentCompetenceById(competence.id);
                newCompetenceUpdateInfo.competenceId = competence.id;
                newCompetenceUpdateInfo.previousAssessmentValue = aCompetence.valueAssessment;
                newCompetenceUpdateInfo.previousLearnValue = aCompetence.valueLearning;
                newCompetenceUpdateInfo.previousLevelLearning = aCompetence.getCompetenceLevel(UpdateType.LEARNING);
                newCompetenceUpdateInfo.previousLevelAssessment = aCompetence.getCompetenceLevel(UpdateType.ASSESSMENT);
                //order of update - low competences should be updated first
                if (situation.isLearning)
                    updateCompetence(competence.id, success, UpdateType.LEARNING, quality, situation.difficulty * competence.weight);
                if (situation.isAssessment)
                    updateCompetence(competence.id, success, UpdateType.ASSESSMENT, quality, situation.difficulty * competence.weight);

                //set update infos 2
                newCompetenceUpdateInfo.newAssessmentValue = aCompetence.valueAssessment;
                newCompetenceUpdateInfo.newLearnValue = aCompetence.valueLearning;
                newCompetenceUpdateInfo.newLevelLearning = aCompetence.getCompetenceLevel(UpdateType.LEARNING);
                newCompetenceUpdateInfo.newLevelAssessment = aCompetence.getCompetenceLevel(UpdateType.ASSESSMENT);
                updateInfoGs.competenceUpdateInformation.Add(newCompetenceUpdateInfo);
            }

            updateInfoGs.printToConsole();
            return updateInfoGs;
        }

        public void resetCompetenceState()
        {
            createInitialValues();
            storeAssessmentState();
        }

        public void createInitialValues()
        {
            DataModel dataModel = CompetenceComponentFunctionality.loadDefaultDataModel();
            difficulties = dataModel.mappings.difficulties;
            competences = new List<AssessmentCompetence>();
            CompetenceComponentSettings settings = CompetenceComponentFunctionality.getSettings();
            float initialValue = (1.0f / (float)settings.NumberOfLevels) / 2.0f;

            foreach (Competence competence in dataModel.elements.competenceList.competences)
            {
                competences.Add(new AssessmentCompetence(competence.id, initialValue));
            }

            foreach (PrerequisiteCompetence comp in dataModel.relations.competenceprerequisites.competenceList)
            {
                AssessmentCompetence competence = getAssessmentCompetenceById(comp.id);
                foreach (Prerequisite prerequisite in comp.prerequisites)
                {
                    competence.prerequisites.Add(getAssessmentCompetenceById(prerequisite.id));
                }
            }

            gamesituations = new List<AssessmentGamesituation>();
            foreach (Gamesituation situation in dataModel.elements.gamesituationList.gamesituations)
            {
                gamesituations.Add(new AssessmentGamesituation(situation, difficulties));
            }
        }

        public void updateDueToForgetting()
        {
            /*
            float linearDecreasion = CompetenceComponentFunctionality.getSettings().LinearDecreasionOfCompetenceValuePerDay;
            foreach (AssessmentCompetence competence in competences)
            {
                DateTime baseTimeAssessment = (competence.timestampAssessment > competence.timestampForgetting ? competence.timestampAssessment : competence.timestampForgetting);
                TimeSpan deltaTimeAssessment = baseTimeAssessment - DateTime.Now;
                competence.valueAssessment -= (float)deltaTimeAssessment.TotalDays * linearDecreasion;
                competence.valueAssessment = Math.Max(competence.valueAssessment, 0f);

                DateTime baseTimeLearning = (competence.timestampLearning > competence.timestampForgetting ? competence.timestampLearning : competence.timestampForgetting);
                TimeSpan deltaTimeLearning = baseTimeLearning - DateTime.Now;
                competence.valueLearning -= (float)deltaTimeLearning.TotalDays * linearDecreasion;
                competence.valueLearning = Math.Max(competence.valueLearning, 0f);

                competence.setTimestamp(Timestamp.FORGETTING);
            }
            */

            float linearDecreasion = CompetenceComponentFunctionality.getSettings().LinearDecreasionOfCompetenceValuePerDay;
            DateTime now = DateTime.Now;
            foreach (AssessmentCompetence competence in competences)
            {
                //Learning Value
                //add value for past forgetting
                DateTime pastTimeLastActionL = competence.timestampLearning;
                DateTime pastTimeLastForgettingUpdate = competence.timestampForgetting;
                competence.valueLearning += getAddValueBasedOnForgetting(competence.valueLearning, pastTimeLastActionL, pastTimeLastForgettingUpdate);
                //substract bigger value for current forgetting
                competence.valueLearning -= getSubstractValueBasedOnForgetting(competence.valueLearning, pastTimeLastActionL, now);
                competence.valueLearning = Math.Max(competence.valueLearning, 0f);


                //Assessment Value
                //add value for past forgetting
                DateTime pastTimeLastActionA = competence.timestampAssessment;
                competence.valueAssessment += getAddValueBasedOnForgetting(competence.valueAssessment, pastTimeLastActionA, pastTimeLastForgettingUpdate);
                //substract bigger value for current forgetting
                competence.valueAssessment -= getSubstractValueBasedOnForgetting(competence.valueAssessment, pastTimeLastActionA, now);
                competence.valueAssessment = Math.Max(competence.valueAssessment, 0f);

                competence.timestampForgetting = now;
            }
        }

        public float getSubstractValueBasedOnForgetting(float initialValue, DateTime pastTimeLastAction, DateTime presentTime)
        {
            float returnValue = 0f;

            float linearDecreasion = CompetenceComponentFunctionality.getSettings().LinearDecreasionOfCompetenceValuePerDay;
            TimeSpan deltaTimeAssessment = presentTime - pastTimeLastAction;
            returnValue = (float)deltaTimeAssessment.TotalDays * linearDecreasion;

            return returnValue;
        }

        public float getAddValueBasedOnForgetting(float initialValue, DateTime pastTimeLastAction, DateTime pastTimeLastForgettingUpdate)
        {
            float returnValue = 0f;
            if (pastTimeLastAction >= pastTimeLastForgettingUpdate)
                return returnValue;

            float linearDecreasion = CompetenceComponentFunctionality.getSettings().LinearDecreasionOfCompetenceValuePerDay;
            TimeSpan deltaTimeAssessment = pastTimeLastForgettingUpdate - pastTimeLastAction;
            returnValue = (float)deltaTimeAssessment.TotalDays * linearDecreasion;

            return returnValue;
        }

        public Dictionary<string, int[]> getCompetenceLevels()
        {
            //assign levels to competences according to number of levels
            float levelWidth = 1.0f / (float)CompetenceComponentFunctionality.getSettings().NumberOfLevels;
            Dictionary<string, int[]> competenceLevels = new Dictionary<string, int[]>();
            foreach (AssessmentCompetence competence in competences)
            {
                int[] level = new int[2];
                level[0] = competence.getCompetenceLevel(UpdateType.ASSESSMENT);
                level[1] = competence.getCompetenceLevel(UpdateType.LEARNING);
                competenceLevels[competence.id] = level;
            }

            return competenceLevels;
        }

        public Dictionary<string, float[]> getCompetenceValues()
        {
            Dictionary<string, float[]> competenceValues = new Dictionary<string, float[]>();
            foreach (AssessmentCompetence competence in competences)
            {
                float[] level = new float[2];
                level[0] = competence.valueAssessment;
                level[1] = competence.valueLearning;
                competenceValues[competence.id] = level;
            }

            return competenceValues;
        }

        public void setCompetenceValues(string competenceId, float learningValue, float assessmentValue)
        {
            AssessmentCompetence comp = getAssessmentCompetenceById(competenceId);
            comp.valueLearning = learningValue;
            comp.valueAssessment = assessmentValue;
            storeAssessmentState();
        }

        //returns two interger: possessed difficulty >=1, and max difficulty
        public int[] getDifficultyRating(float difficulty)
        {
            int[] returnValue = new int[2];
            //count difficulties<=difficulty
            int countVariable = 0;
            foreach (Difficulty diff in difficulties.difficultyList)
            {
                if (diff.weight <= difficulty)
                {
                    countVariable++;
                }
            }
            returnValue[0] = countVariable;
            returnValue[1] = difficulties.difficultyList.Count;
            return returnValue;
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
                CompetenceComponentFunctionality.loggingCC("         -" + competence.id + "::" + Math.Round(competence.valueAssessment, 2) + "::" + competence.timestampAssessment.ToString());
            }

        }

        #endregion
    }

    public class AssessmentCompetence
    {
        #region Fields
        public string id;
        public DateTime timestampAssessment;
        public DateTime timestampLearning;
        public DateTime timestampForgetting;
        public float valueAssessment;
        public float valueLearning;
        public List<AssessmentCompetence> prerequisites = new List<AssessmentCompetence>();
        #endregion
        #region Methods

        public float getMinimumPrerequisiteValueOrOneIfThereAreNoPrerequisites(UpdateType type)
        {
            float retVal = 1f;
            foreach (AssessmentCompetence asscomp in prerequisites)
            {
                if(type.Equals(UpdateType.ASSESSMENT) && retVal > asscomp.valueAssessment)
                {
                    retVal = asscomp.valueAssessment;
                }else if (type.Equals(UpdateType.LEARNING) && retVal > asscomp.valueLearning)
                {
                    retVal = asscomp.valueLearning;
                }
            }

            return retVal;
        }

        public void setTimestamp(Timestamp stamp)
        {
            switch (stamp)
            {
                case Timestamp.ASSESSMENT:
                    timestampAssessment = DateTime.Now;
                    break;
                case Timestamp.LEARNING:
                    timestampLearning = DateTime.Now;
                    break;
                case Timestamp.FORGETTING:
                    timestampForgetting = DateTime.Now;
                    break;
            }
        }

        public int getCompetenceLevel(UpdateType type)
        {
            return getCompetenceLevel(type, CompetenceComponentFunctionality.getSettings().NumberOfLevels);
        }

        //0 <= level < numberOfLevels
        public int getCompetenceLevel(UpdateType type, int numberOfLevels)
        {
            float levelWidth = 1.0f / (float)numberOfLevels;

            float baseValue = (type.Equals(UpdateType.ASSESSMENT)) ? valueAssessment : valueLearning;
            int level = (int)Math.Floor(baseValue / levelWidth);
            level = Math.Min(level, numberOfLevels - 1);

            return level;
        }

        public bool isPauseTimeOver(UpdateType type)
        {
            int level = getCompetenceLevel(type);
            int basePauseTime = CompetenceComponentFunctionality.getSettings().CompetencePauseTimeInSeconds;
            Timestamp stamp = (type.Equals(UpdateType.ASSESSMENT)) ? Timestamp.ASSESSMENT : Timestamp.LEARNING;
            DateTime baseTime = (stamp.Equals(Timestamp.ASSESSMENT)) ? timestampAssessment : timestampLearning;
            DateTime overTime = baseTime.AddSeconds(basePauseTime * (level + 1));
            if (overTime > DateTime.Now)
            {
                return false;
            }
            return true;
        }

        public float getRecommendationValue(UpdateType type)
        {
            DateTime baseTime = (type.Equals(UpdateType.ASSESSMENT)) ? timestampAssessment : timestampLearning;
            float inactiveTimeInDays = (float)(DateTime.Now - baseTime).TotalDays;
            float pauseTimeOver = isPauseTimeOver(type) ? 1f : 0;
            float maxLevels = (float)CompetenceComponentFunctionality.getSettings().NumberOfLevels;
            float returnValue = 0;

            if (type.Equals(UpdateType.ASSESSMENT))
            {
                if (CompetenceComponentFunctionality.getSettings().Phase.Equals(CompetenceComponentPhase.ASSESSMENT))
                {
                    returnValue = inactiveTimeInDays * pauseTimeOver;
                }
                else
                {
                    returnValue = (valueLearning - valueAssessment) * maxLevels * inactiveTimeInDays * pauseTimeOver;
                }
            }
            else if (type.Equals(UpdateType.LEARNING))
            {
                float minPrereqValue = getMinimumPrerequisiteValueOrOneIfThereAreNoPrerequisites(UpdateType.LEARNING);
                float prereqMinValueSmallerFactor  = (minPrereqValue > valueLearning) ? 1f : 0f;
                returnValue = ((maxLevels - (float)getCompetenceLevel(UpdateType.LEARNING)) / maxLevels) * inactiveTimeInDays * pauseTimeOver * prereqMinValueSmallerFactor;
            }

            return returnValue;
        }

        #endregion
        #region Constructors
        public AssessmentCompetence(string id, float value)
        {
            this.id = id;
            int pauseTimeSeconds = CompetenceComponentFunctionality.getSettings().CompetencePauseTimeInSeconds;
            this.timestampAssessment = DateTime.Now.AddSeconds(-1 - pauseTimeSeconds);
            this.timestampLearning = this.timestampAssessment;
            this.timestampForgetting = this.timestampAssessment;
            this.valueAssessment = value;
            this.valueLearning = value;
        }
        #endregion
    }

    public class AssessmentGamesituation
    {
        #region Fields
        public string id;
        public float difficulty;
        public bool isLearning;
        public bool isAssessment;
        public List<AssessmentGamesituationCompetence> competences = new List<AssessmentGamesituationCompetence>();
        #endregion
        #region Constructors
        public AssessmentGamesituation(Gamesituation gamesituation, Difficulties difficulties)
        {
            id = gamesituation.id;
            difficulty = difficulties.getDifficulty(gamesituation.difficulty);
            if (difficulty < 0)
            {
                throw new Exception("Can't find difficulty '" + gamesituation.difficulty + "' in data model, section datamodel-mappings-difficulties-difficulty-id.");
            }
            foreach (GamesituationCompetence competence in gamesituation.competences)
            {
                competences.Add(new AssessmentGamesituationCompetence(competence));
            }
            isLearning = gamesituation.isLearning;
            isAssessment = gamesituation.isAssessment;
        }
        #endregion
        #region Methods

        public float getRecommendationValue(UpdateType type)
        {
            float returnValue = 0;
            foreach (AssessmentGamesituationCompetence competence in competences)
            {
                AssessmentCompetence assCompetence = CompetenceComponentFunctionality.assessmentObject.getAssessmentCompetenceById(competence.id);
                returnValue += competence.weight * assCompetence.getRecommendationValue(type) * getDifficultyCompetenceLevelFactor(assCompetence, type);
                //print(returnValue.ToString() + "[" + getDifficultyCompetenceLevelFactor(assCompetence, type).ToString() + "]");
            }


            return returnValue;
        }

        public float getDifficultyCompetenceLevelFactor(AssessmentCompetence competence, UpdateType type)
        {
            //returns two interger: possessed difficulty >=1, and <= max difficulty
            int[] difficultyRating = CompetenceComponentFunctionality.assessmentObject.getDifficultyRating(difficulty);
            //get recommendation level: >=1 AND <= max difficulty 
            int level = 1 + competence.getCompetenceLevel(type, difficultyRating[1]);
            float returnValue = 1.5f - ((float)Math.Abs(level - difficultyRating[0])) / ((float)difficultyRating[1] - 1f);

            return returnValue;
        }

        #endregion
        #region Helpermethods
        public void print(string additionalInformation = null)
        {
            string txt = "GS: '" + id + "' ";
            if (additionalInformation != null)
                txt += "(" + additionalInformation + ")";
            CompetenceComponentFunctionality.loggingCC(txt);
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

    public class RecommendationObject
    {
        #region Fields
        #endregion
        #region Methods

        public static AssessmentCompetence getCompetenceRecommendation(List<AssessmentCompetence> competences, UpdateType type)
        {
            #region OLDMETHOD
            /* OLD METHOD
            //assign levels to competences according to number of levels
            Dictionary<AssessmentCompetence, int> competenceLevels = new Dictionary<AssessmentCompetence, int>();
            foreach (AssessmentCompetence competence in competences)
            {
                int level = competence.getCompetenceLevel(type);
                competenceLevels[competence] = level;
            }

            //get competences with lowest levels and pause time over
            List<AssessmentCompetence> minLevelCompetences = new List<AssessmentCompetence>();
            int minLevel = int.MaxValue;
            foreach (AssessmentCompetence competence in competences)
            {
                if (competence.isPauseTimeOver(type))
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
            }

            //get competence with oldest timestamp
            List<AssessmentCompetence> minLevelOldestTimestampCompetence = new List<AssessmentCompetence>();
            DateTime oldestTimeStamp = DateTime.Now;
            foreach (AssessmentCompetence competence in minLevelCompetences)
            {
                DateTime dateTime = (type.Equals(UpdateType.ASSESSMENT)) ? competence.timestampAssessment : competence.timestampLearning;
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

            //select random competence from min level oldest timestamp competences if there is one
            if (minLevelOldestTimestampCompetence.Count == 0)
                return null;
            Random rnd = new Random();
            int r = rnd.Next(minLevelOldestTimestampCompetence.Count);
            AssessmentCompetence selectedCompetence = minLevelOldestTimestampCompetence[r];
            */
            #endregion
            AssessmentCompetence selectedCompetence = null;
            foreach (AssessmentCompetence competence in competences)
            {
                if (selectedCompetence == null || selectedCompetence.getRecommendationValue(type) < competence.getRecommendationValue(type))
                {
                    selectedCompetence = competence;
                }
            }

            return selectedCompetence;
        }

        public static List<AssessmentGamesituation> getGamesituationRecommendation(List<AssessmentCompetence> competences, List<AssessmentGamesituation> gamesituations)
        {
            //CompetenceComponentFunctionality.loggingCC("Getting GS recommendation");
            //check: do assessment - when in assessment phase OR when assessment value of one competence is high enought
            bool doAssessment = CompetenceComponentFunctionality.getSettings().Phase.Equals(CompetenceComponentPhase.ASSESSMENT) || getCompetenceRecommendation(competences, UpdateType.ASSESSMENT).getRecommendationValue(UpdateType.ASSESSMENT) >= CompetenceComponentFunctionality.getSettings().ThreasholdRecommendationSelection;
            float currentGamesituationValue = 0;

            Dictionary<AssessmentGamesituation, float> gameSituationsAndValues = new Dictionary<AssessmentGamesituation, float>();

            foreach (AssessmentGamesituation situation in gamesituations)
            {
                if ((doAssessment && situation.isAssessment) || (!doAssessment && situation.isLearning))  //do assessment || do learning
                {
                    currentGamesituationValue = situation.getRecommendationValue(doAssessment ? UpdateType.ASSESSMENT : UpdateType.LEARNING);
                    gameSituationsAndValues[situation] = currentGamesituationValue;
                }
            }

            //order game situation based on values
            List<AssessmentGamesituation> GSs = new List<AssessmentGamesituation>();
            foreach (KeyValuePair<AssessmentGamesituation, float> item in gameSituationsAndValues.OrderByDescending(key => key.Value))
            {
                GSs.Add(item.Key);
            }



            return GSs;
        }

        #endregion
    }

    #endregion
    #region ReturnObjects
    public class GamesituationUpdateDescription
    {
        public string gamesituationId;
        public List<CompetenceUpdateDescription> competenceUpdateInformation = new List<CompetenceUpdateDescription>();

        internal void printToConsole()
        {
            CompetenceComponentFunctionality.loggingCC("===============================");
            CompetenceComponentFunctionality.loggingCC(gamesituationId+":");
            CompetenceComponentFunctionality.loggingCC("-----------------");
            foreach (CompetenceUpdateDescription info in competenceUpdateInformation)
            {
                info.printToConsole();
            }
            CompetenceComponentFunctionality.loggingCC("===============================");
        }
    }

    public class CompetenceUpdateDescription
    {
        public string competenceId;
        public float previousLearnValue;
        public float newLearnValue;
        public float previousAssessmentValue;
        public float newAssessmentValue;
        public int previousLevelLearning;
        public int newLevelLearning;
        public int previousLevelAssessment;
        public int newLevelAssessment;

        internal void printToConsole()
        {
            CompetenceComponentFunctionality.loggingCC(competenceId + ":");
            CompetenceComponentFunctionality.loggingCC("   LV:"+previousLearnValue+"->"+newLearnValue);
            CompetenceComponentFunctionality.loggingCC("   LL:" + previousLevelLearning + "->" + newLevelLearning);
            CompetenceComponentFunctionality.loggingCC("   AV:" + previousAssessmentValue + "->" + newAssessmentValue);
            CompetenceComponentFunctionality.loggingCC("   AL:" + previousLevelAssessment + "->" + newLevelAssessment);
        }
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
