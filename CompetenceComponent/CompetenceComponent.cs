/*
  Copyright 2016 TUGraz, http://www.tugraz.at/
  
  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  This project has received funding from the European Union�s Horizon
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

// <summary>Implements the CompetenceComponent class</summary>
namespace CompetenceComponentNamespace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AssetManagerPackage;
    using AssetPackage;

    /// <summary>
    /// An asset.
    /// </summary>
    public class CompetenceComponent : BaseAsset
    {
        #region Fields

        /// <summary>
        /// Options for controlling the operation.
        /// </summary>
        private CompetenceComponentSettings settings = null;

        /// <summary>
        /// Instance of the class CompetenceComponent - Singelton pattern
        /// </summary>
        static readonly CompetenceComponent instance = new CompetenceComponent();

        private bool isInitialized=false;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the CompetenceComponent.Asset class.
        /// </summary>
        private CompetenceComponent()
            : base()
        {
            //! Create Settings and let it's BaseSettings class assign Defaultvalues where it can.
            // 
            settings = new CompetenceComponentSettings();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets options for controlling the operation.
        /// </summary>
        ///
        /// <remarks>   Besides the toXml() and fromXml() methods, we never use this property but use
        ///                it's correctly typed backing field 'settings' instead. </remarks>
        /// <remarks> This property should go into each asset having Settings of its own. </remarks>
        /// <remarks>   The actual class used should be derived from BaseAsset (and not directly from
        ///             ISetting). </remarks>
        ///
        /// <value>
        /// The settings.
        /// </value>
        public override ISettings Settings
        {
            get
            {
                return settings;
            }
            set
            {
                settings = (value as CompetenceComponentSettings);
            }
        }

        /// <summary>
        /// Getter for Instance of the DomainModelAsset - Singelton pattern
        /// </summary>
        public static CompetenceComponent Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Method for updating a single competence
        /// </summary>
        /// <param name="competence">string id of the competence for the update</param>
        /// <param name="success">true if the competence is upgraded, false if it is downgraded</param>
        private void updateCompetence(string competence, bool success, UpdateType type)
        {
            if (!isInitialized)
                initialize();


            if (!isInitialized)
                return;

            CompetenceComponentFunctionality.updateDueToForgetting();
            CompetenceComponentFunctionality.UpdateCompetence(competence, success, type);
        }

        /// <summary>
        /// Method for updating a single game situation
        /// </summary>
        /// <param name="gamesituation">string id of the gamesituation for the update</param>
        /// <param name="success">true if the competence is upgraded, false if it is downgraded</param>
        public GamesituationUpdateDescription updateGamesituation(string gamesituation, bool success, float quality = 1.0f)
        {
            if (quality < 0 || quality >1)
            {
                throw new Exception("Quality values for game situation updtae need to be between and including 0 and 1.");
            }

            if (!isInitialized)
                initialize();


            if (!isInitialized)
                return null;

            CompetenceComponentFunctionality.updateDueToForgetting();
            return CompetenceComponentFunctionality.UpdateGamesituation(gamesituation, success, quality);
        }

        /// <summary>
        /// Request the string id of the next competence to test/train
        /// </summary>
        /// <returns> the string id of the competence to train/test</returns>
        private string getCompetenceRecommendation(UpdateType type)
        {
            if (!isInitialized)
                initialize();
            
            if (!isInitialized)
                return null;

            CompetenceComponentFunctionality.updateDueToForgetting();
            return CompetenceComponentFunctionality.GetCompetenceRecommendation(type);
        }
        
        /// <summary>
        /// Request the string id of the next gamesituation to test/train
        /// </summary>
        /// <returns> the string id of the gamesituation to train/test</returns>
        public List<string> getGamesituationRecommendation(int quantity=1)
        {
            if (!isInitialized)
                initialize();

            if (!isInitialized)
                return null;

            CompetenceComponentFunctionality.updateDueToForgetting();
            return CompetenceComponentFunctionality.GetGamesituationRecommendation(quantity);
        }

        /// <summary>
        /// Is loading the dataModel and preparing the component's functionality
        /// </summary>
        public void initialize()
        {
            try
            {
                CompetenceComponentFunctionality.Initialize();
                isInitialized = true;
            }
            catch (DataModelNotFoundException e)
            {
                CompetenceComponentFunctionality.loggingCC("Can't proceed - data model can not be loaded. File can not be found!");
            }
        }

        public void resetCompetenceState()
        {
            if (!isInitialized)
                initialize();
            
            if (!isInitialized)
                return;

            CompetenceComponentFunctionality.resetCompetenceState();
        }

        /// <summary>
        /// Returns the currently possessed competence levels
        /// </summary>
        /// <returns>
        /// return[0]...assessment
        /// return[1]...learning
        /// </returns>
        public Dictionary<string,int[]> getCompetenceLevels()
        {

            if (!isInitialized)
                initialize();

            if (!isInitialized)
                return null;

            CompetenceComponentFunctionality.updateDueToForgetting();
            return CompetenceComponentFunctionality.getCompetencelevels();
        }

        public int getCompetenceLevel(string competenceId, UpdateType type)
        {

            if (!isInitialized)
                initialize();

            if (!isInitialized)
                return -1;

            int pos = type.Equals(UpdateType.ASSESSMENT) ? 0 : 1;

            int level = getCompetenceLevels()[competenceId][pos];
            return level;
        }

        /// <summary>
        /// Returns the competence id and its competence values in a dictionary.
        /// </summary>
        /// <returns>
        /// float[0]=learning value
        /// float[1]=assessment value
        /// </returns>
        public Dictionary<string,float[]> getCompetenceValues()
        {
            if (!isInitialized)
                initialize();

            if (!isInitialized)
                return null;

            CompetenceComponentFunctionality.updateDueToForgetting();
            return CompetenceComponentFunctionality.getCompetenceValues();
        }

        public void setCompetenceValues(string competenceId, float learningValue, float assessmentValue)
        {
            if (!isInitialized)
                initialize();

            if (!isInitialized)
                return;

            CompetenceComponentFunctionality.setCompetenceValues(competenceId, learningValue, assessmentValue);
        }

        #endregion Methods

        #region internal Methods

        /// <summary>
        /// Wrapper method for getting the getInterface method of the base Asset
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <returns>Corresponding Interface</returns>
        internal T getInterfaceFromAsset<T>()
        {
            return this.getInterface<T>();
        }

        #endregion internal Methods

        #region TestMethods

        public DataModel getDataModel()
        {
            return CompetenceComponentFunctionality.loadDefaultDataModel();
        }

        public CompetenceAssessmentObject getAssessmentObject()
        {
            return CompetenceComponentFunctionality.getAssessmentObject();
        }



        #endregion
    }
}