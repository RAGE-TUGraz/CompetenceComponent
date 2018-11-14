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

using System;
using CompetenceComponentNamespace;
using AssetManagerPackage;
using System.Collections.Generic;

namespace TestCompetenceComponent
{
    class Program
    {
        static void Main(string[] args)
        {
            test();
            /*
            //create Asset Manager and assign Bridge
            AssetManager am = AssetManager.Instance;
            am.Bridge = new Bridge();

            //create Component to be tested and write its settings
            CompetenceComponent cc = CompetenceComponent.Instance;
            CompetenceComponentSettings ccs = new CompetenceComponentSettings();
            ccs.NumberOfLevels = 3;
            ccs.LinearDecreasionOfCompetenceValuePerDay = 0.1f;
            ccs.SourceFile = "dataModel2.xml";
            ccs.CompetencePauseTimeInSeconds = 5;
            ccs.Phase = CompetenceComponentPhase.DEFAULT;
            ccs.ThreasholdRecommendationSelection = 1f/(60f*60f);
            cc.Settings = ccs;
            //*/

            //test loading/storing assessment object 
            /*
            cc.Initialize();
            cc.getAssessmentObject().printToConsole();
            cc.Update("C2",true);
            Console.WriteLine(cc.getCompetenceLevel("C2"));
            */

            //cc.getDataModel().printToCommandline();

            /*
            DataModel dm = new DataModel();
            dm.addCompetence("C1");
            dm.addCompetence("C2");
            dm.addCompetence("C3");
            dm.addPrerequisites("C3",new List<string>(new string[]{ "C2", "C1"}));
            dm.addDifficulty("easy",0.2f);
            Dictionary<string, float> competencies = new Dictionary<string, float>();
            competencies["C1"] = 0.5f;
            competencies["C2"] = 1.5f;
            dm.addGamesituation("GS1","easy",true, true, competencies);
            dm.printToCommandline();

            //*/


            /*
            cc.resetCompetenceState();
            printCompetenceLevels(cc.getCompetenceLevels());

            cc.setCompetenceValues(new List<string>(cc.getCompetenceValues().Keys)[0],1,1);
            Console.WriteLine("Load Value:"+cc.getCompetenceValues()[new List<string>(cc.getCompetenceValues().Keys)[0]][0]);

            bool doGamesituations = true;

            bool doLoop = true;
            if (doGamesituations)
            {
                //update according to gamesituations
                while (doLoop)
                {
                    string gamesituation = cc.getGamesituationRecommendation()[0];
                    if (gamesituation == null)
                    {
                        Console.WriteLine("There is no gamesituation to present. Try again after some time with Enter.");
                        Console.ReadLine();
                    }
                    else
                    {
                        Console.WriteLine("Presented Gamesituation: '" + gamesituation + "'");
                        Console.WriteLine("[e]...Exit   [p]...positive evidence   [n]...negative evidence");
                        ConsoleKeyInfo input = Console.ReadKey();
                        Console.WriteLine("");
                        if (input.KeyChar.Equals('e'))
                        {
                            doLoop = false;
                        }
                        else
                        {
                            switch (input.KeyChar)
                            {
                                case 'p':
                                    cc.updateGamesituation(gamesituation, true);
                                    break;
                                case 'n':
                                    cc.updateGamesituation(gamesituation, false);
                                    break;
                                default:
                                    continue;
                            }
                            printCompetenceLevels(cc.getCompetenceLevels());
                        }
                    }
                }

            }
            //*/

            Console.WriteLine("Press enter to exit....");
            Console.ReadLine();
        }


        public static void printCompetenceLevels(Dictionary<string, int[]> levels)
        {
            foreach (string competence in levels.Keys)
            {
                Console.WriteLine(competence+":  A:"+levels[competence][0] + "_L:" + levels[competence][1] + "/"+ (((CompetenceComponentSettings)CompetenceComponent.Instance.Settings).NumberOfLevels-1).ToString());
            }
        }

        public static void test()
        {
            testRun(true, true);
            testRun(true, false);
            testRun(false, true);
            testRun(false, false);
        }

        public static void testRun(bool dm1, bool set)
        {
            //PlayerPrefs.DeleteAll();

            //Bridge
            Bridge bridge = new Bridge();

            //Create two data models that share one competence
            DataModel dataModel = new DataModel();
            string dmName = "";
            string playerName = "";
            if (dm1)
            {
                dataModel.addCompetence("C1");
                dataModel.addCompetence("C2");
                dmName = "firstDMName.xml";
                playerName = "player_dm1";
            }
            else
            {
                dataModel.addCompetence("C1");
                dataModel.addCompetence("C3");
                dmName = "secondDMName.xml";
                playerName = "player_dm2";
            }
            

           
            AssetManager am = AssetManager.Instance;
            //Creation of the Asset Manager

            am.Bridge = new Bridge();
            //Set the bridge of the Asset Manager(Bridge implementation needs to be in place)


            //Save data models
            dataModel.storeToFile(dmName);

            CompetenceComponent cc = CompetenceComponent.Instance;
            //Creation of the Asset

            CompetenceComponentSettings ccs = new CompetenceComponentSettings();
            //Creation of the Asset’s settings. In the following, the setting-values are adjusted.

            ccs.NumberOfLevels = 3;
            //Set the number of levels for the competences -learning and assessment values.

            ccs.LinearDecreasionOfCompetenceValuePerDay = 0.1f;
            //Set the decreation rate(forgetting) -this is done linearely

            ccs.SourceFile = dmName;
            //Set the path of the xml-data model.

            ccs.CompetenceValueStoragePrefix = playerName;
            //Set the prefix of the file storing the competence values.

            ccs.CompetencePauseTimeInSeconds = 5;
            //The pause time for a competence on the lowest level.This quantity is introduced for the Leitner-System.

            ccs.Phase = CompetenceComponentPhase.DEFAULT;
            //Can either be CompetenceComponentPhase.DEFAULT or CompetenceComponentPhase.ASSESSMENT.In the default state, learning and assessment game situations are selected.In assessment state, only assessment game situations are selected.

            ccs.ThreasholdRecommendationSelection = 1f / (60f * 60f);
            //Threshold for Assessment value of game situations.If this value is exceeded a assessment game situation is selected.Otherwise, a learning game situation is selected.

            cc.Settings = ccs;
            //The Assets’s settings are stored in the Asset.

            
            cc.initialize();

            if (dm1 && set)
            {
                cc.setCompetenceValues("C1", 0.5f, 0.5f);
                cc.setCompetenceValues("C2", 0.75f, 0.75f);
            }
            else if(set)
            {
                cc.setCompetenceValues("C1", 0.75f, 0.75f);
                cc.setCompetenceValues("C3", 0.75f, 0.75f);
            }

            Dictionary<string, float[]> values = cc.getCompetenceValues();


            foreach (KeyValuePair<string, float[]> pair in values)
            {
                Console.WriteLine("{0} - learning: {1} - assessment: {2}", pair.Key, pair.Value[0], pair.Value[1]);
            }
        }
    }
}