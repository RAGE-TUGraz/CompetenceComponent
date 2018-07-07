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

namespace CompetenceComponentNamespace
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using AssetPackage;

    public enum CompetenceComponentPhase {ASSESSMENT, DEFAULT};

    /// <summary>
    /// An asset settings.
    /// 
    /// BaseSettings contains the (de-)serialization methods.
    /// </summary>
    public class CompetenceComponentSettings : BaseSettings
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the CompetenceComponent.AssetSettings class.
        /// </summary>
        public CompetenceComponentSettings()
            : base()
        {
            // Set Default values here.
            NumberOfLevels = 3;
            LinearDecreasionOfCompetenceValuePerDay = 0.1f;
            SourceFile = "dataModel.xml";
            Phase = CompetenceComponentPhase.DEFAULT;
            CompetencePauseTimeInSeconds = 60*24;
            ThreasholdRecommendationSelection = 24/(24*60*2); //half minute
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Number of levels within a competence
        /// </summary>
        ///
        /// <value>
        /// integer >= 1
        /// </value>
        [XmlElement()]
        public int NumberOfLevels
        {
            get;
            set;
        }

        /// <summary>
        /// Number of levels within a competence
        /// </summary>
        ///
        /// <value>
        /// integer >= 1
        /// </value>
        [XmlElement()]
        public CompetenceComponentPhase Phase
        {
            get;
            set;
        }

        /// Describes the pause time until a competence on level 0 is tested/learned again, on higher a multiple of it is taken
        /// </summary>
        ///
        /// <value>
        /// integer >= 1
        /// </value>
        [XmlElement()]
        public int CompetencePauseTimeInSeconds
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies how strong the competence value is decreased within 24 hours, linear
        /// </summary>
        ///
        /// <value>
        /// float 0<value<1
        /// </value>
        [XmlElement()]
        public float LinearDecreasionOfCompetenceValuePerDay
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies from which file to load the data model xml
        /// </summary>
        ///
        /// <value>
        /// string
        /// </value>
        [XmlElement()]
        public string SourceFile
        {
            get;
            set;
        }


        /// <summary>
        /// Specifies which value need to be reached by competencies to be select an assessment game situation
        /// </summary>
        ///
        /// <value>
        /// float 0<=value
        /// </value>
        [XmlElement()]
        public float ThreasholdRecommendationSelection
        {
            get;
            set;
        }


        #endregion Properties
    }
}
