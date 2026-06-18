namespace TemplateControl
{
    /// <summary>
    /// Defines the type of pipe fitting applied to the start or end of a <see cref="PipeControl"/>.
    /// </summary>
    public enum FittingType
    {
        /// <summary>No fitting (open pipe end).</summary>
        None,

        /// <summary>Flange plate fitting.</summary>
        Flange,

        /// <summary>90° elbow (positive direction).</summary>
        Elbow90,

        /// <summary>90° elbow (negative direction).</summary>
        ElbowMinus90,

        /// <summary>45° elbow (positive direction).</summary>
        Elbow45,

        /// <summary>45° elbow (negative direction).</summary>
        ElbowMinus45
    }
}
