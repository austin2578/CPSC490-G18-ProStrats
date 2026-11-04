// The three weapon families for the project.
public enum WeaponFamily { Sword, Bow, Wand }

public enum ScalingGrade { S, A, B, C, D, E }

public static class ScalingGradeExtensions
{
    // Placeholder numbers: damage gained per attribute point at each grade. Tune in playtesting.
    public static float Multiplier(this ScalingGrade grade)
    {
        switch (grade)
        {
            case ScalingGrade.S: return 1.0f;
            case ScalingGrade.A: return 0.8f;
            case ScalingGrade.B: return 0.6f;
            case ScalingGrade.C: return 0.4f;
            case ScalingGrade.D: return 0.25f;
            default: return 0.1f; // E
        }
    }
}
