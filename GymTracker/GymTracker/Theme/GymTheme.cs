using MudBlazor;

namespace GymTracker.Theme;

public static class GymTheme
{
    public static MudTheme Theme = new MudTheme()
    {
        PaletteDark = new PaletteDark()
        {
            Primary = "#22c55e",
            Secondary = "#06b6d4",

            Background = "#020617",
            Surface = "#0f172a",

            AppbarBackground = "#020617",
            DrawerBackground = "#020617",

            TextPrimary = "#e5e7eb",
            TextSecondary = "#94a3b8",

            Success = "#22c55e",
            Warning = "#f59e0b",
            Error = "#ef4444",
            Info = "#38bdf8"
        },

        Typography = new Typography()
        {
            Default = new DefaultTypography()
            {
                FontFamily = new[] { "Oswald", "sans-serif" },
                FontSize = "1rem"
            },

            Body1 = new Body1Typography()
            {
                FontSize = "1.1rem"
            },

            H1 = new H1Typography()
            {
                FontSize = "2.4rem",
            },

            Button = new ButtonTypography()
            {
                FontSize = "1rem",
            }
        }
        ,

        LayoutProperties = new LayoutProperties()
        {
            DefaultBorderRadius = "6px"
        }
    };
}