using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntScaleLib
{
    public class Weight
    {
        public const byte DATAPAGE01_BODY_WEIGHT = 1;
        public const byte DATAPAGE02_BODY_COMPOSITION_PERCENTAGE = 2;
        public const byte DATAPAGE03_METABOLIC_INFORMATION = 3;
        public const byte DATAPAGE04_BODY_COMPOSITION_MASS = 4;
        public const byte DATAPAGE58_USER_PROFILE = 58;
        
        public const byte DATAPAGE01_BODY_WEIGHT_MASK = 0x01;
        public const byte DATAPAGE02_BODY_COMPOSITION_PERCENTAGE_MASK = 0x02;
        public const byte DATAPAGE03_METABOLIC_INFORMATION_MASK = 0x04;
        public const byte DATAPAGE04_BODY_COMPOSITION_MASS_MASK = 0x08;
        public const byte DATAPAGE58_USER_PROFILE_MASK = 0x10;

        public const byte EXPECTED_DATAPAGES =  DATAPAGE01_BODY_WEIGHT_MASK |
                                                DATAPAGE02_BODY_COMPOSITION_PERCENTAGE_MASK |
                                                DATAPAGE03_METABOLIC_INFORMATION_MASK |
                                                DATAPAGE04_BODY_COMPOSITION_MASS_MASK |
                                                DATAPAGE58_USER_PROFILE_MASK;
        DateTime timestamp;
        UInt16 userProfile;
        double weight;
        string gender;
        byte age;
        byte height;
        double hydrationPercentage;
        double bodyFatPercentage;
        double activeMetabolicRate;
        double basalMetabolicRate;
        double muscleMass;
        double boneMass;
        byte dataPages;
        public byte DataPages { get => dataPages; }

        public void UpdateDataPage1BodyWeight(UInt16 userProfile, double weight)
        {
            this.timestamp = DateTime.Now;
            this.userProfile = userProfile;
            this.weight = weight;
            this.dataPages |= DATAPAGE01_BODY_WEIGHT_MASK;
        }

        public void UpdateDataPage2BodyCompositionPercentage(UInt16 userProfile, double hydrationPercentage, double bodyFatPercentage)
        {
            this.timestamp = DateTime.Now;
            this.userProfile = userProfile;
            this.hydrationPercentage = hydrationPercentage;
            this.bodyFatPercentage = bodyFatPercentage;
            this.dataPages |= DATAPAGE02_BODY_COMPOSITION_PERCENTAGE_MASK;
        }

        public void UpdateDataPage3MetabolicInformation(UInt16 userProfile, double activeMetabolicRate, double basalMetabolicRate)
        {
            this.timestamp = DateTime.Now;
            this.userProfile = userProfile;
            this.activeMetabolicRate = activeMetabolicRate;
            this.basalMetabolicRate = basalMetabolicRate;
            this.dataPages |= DATAPAGE03_METABOLIC_INFORMATION_MASK;
        }

        public void UpdateDataPage4BodyCompositionMass(UInt16 userProfile, double muscleMass, double boneMass)
        {
            this.timestamp = DateTime.Now;
            this.userProfile = userProfile;
            this.muscleMass = muscleMass;
            this.boneMass = boneMass;
            this.dataPages |= DATAPAGE04_BODY_COMPOSITION_MASS_MASK;
        }

        public void UpdateDataPage58UserProfile(UInt16 userProfile, string gender, byte age, byte height)
        {
            this.timestamp = DateTime.Now;
            this.userProfile = userProfile;
            this.gender = gender;
            this.age = age;
            this.height = height;
            this.dataPages |= Weight.DATAPAGE58_USER_PROFILE_MASK;
        }

        public string MakeJSON()
        {
            string s = "{ ";
            s += $"\"timestamp\": \"{timestamp.ToString("s")}\", ";
            s += $"\"userProfile\": {userProfile}, ";
            s += $"\"weight\": {weight}, ";
            s += $"\"gender\": \"{gender}\", ";
            s += $"\"age\": {age}, ";
            s += $"\"height\": {height}, ";
            s += $"\"hydrationPercentage\": {hydrationPercentage}, ";
            s += $"\"bodyFatPercentage\": {bodyFatPercentage}, ";
            s += $"\"activeMetabolicRate\": {activeMetabolicRate}, ";
            s += $"\"basalMetabolicRate\": {basalMetabolicRate}, ";
            s += $"\"muscleMass\": {muscleMass}, ";
            s += $"\"boneMass\": {boneMass}";
            s += " }";

            return s;
        }
    }
}
