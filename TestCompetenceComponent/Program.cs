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

            //test loading/storing assessment object 
            /*
            cc.Initialize();
            cc.getAssessmentObject().printToConsole();
            cc.Update("C2",true);
            Console.WriteLine(cc.getCompetenceLevel("C2"));
            */

            //cc.getDataModel().printToCommandline();

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


            //*
            cc.ResetCompetenceState();
            printCompetenceLevels(cc.getCompetenceLevels());

            bool doGamesituations = true;

            bool doLoop = true;
            if (doGamesituations)
            {
                //update according to gamesituations
                while (doLoop)
                {
                    string gamesituation = cc.GetGamesituationRecommendation()[0];
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
                                    cc.UpdateGamesituation(gamesituation, true);
                                    break;
                                case 'n':
                                    cc.UpdateGamesituation(gamesituation, false);
                                    break;
                                default:
                                    continue;
                            }
                            printCompetenceLevels(cc.getCompetenceLevels());
                        }
                    }
                }

            }
            else
            {
                // update according to competencies
                while (doLoop)
                {
                    string competence = cc.GetCompetenceRecommendation(UpdateType.ASSESSMENT);
                    if (competence == null)
                    {
                        Console.WriteLine("There is no competence to present. Try again after some time with Enter.");
                        Console.ReadLine();
                    }
                    else
                    {
                        Console.WriteLine("Presented Competence: '" + competence + "'");
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
                                    cc.UpdateCompetence(competence, true, UpdateType.ASSESSMENT);
                                    break;
                                case 'n':
                                    cc.UpdateCompetence(competence, false, UpdateType.ASSESSMENT);
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
    }
}