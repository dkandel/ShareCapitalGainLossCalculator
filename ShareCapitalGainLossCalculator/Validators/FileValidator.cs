namespace ShareCapitalGainLossCalculator.Validators;

public static class FileValidator
{
    public static (bool IsValid, string ErrorMessage) Validate(this IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return (false, "File is required.");
        }

        // Validate file extension
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (fileExtension != ".csv")
        {
            return (false, "Only CSV files are allowed.");
        }

        // Validate ContentType
        if (file.ContentType != "text/csv" && file.ContentType != "application/vnd.ms-excel")
        {
            return (false, "Invalid file type. Only CSV files are allowed.");
        }

        return (true, string.Empty);
    }
}
