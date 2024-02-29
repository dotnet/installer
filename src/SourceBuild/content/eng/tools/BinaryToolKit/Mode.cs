namespace BinaryToolKit;

public static class Mode
{
    public enum ModeOptions
    {
        both,
        b,
        validate,
        v,
        clean,
        c
    }

    public static ModeOptions GetFullMode(ModeOptions modeOption)
    {
        return (modeOption) switch
        {
            ModeOptions.both => modeOption,
            ModeOptions.b => ModeOptions.both,
            ModeOptions.validate => modeOption,
            ModeOptions.v => ModeOptions.validate,
            ModeOptions.clean => modeOption,
            ModeOptions.c => ModeOptions.clean,
            _ => throw new ArgumentException("Invalid mode")
        };
    }
}