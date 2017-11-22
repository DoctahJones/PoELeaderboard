namespace PoELeaderboard
{
    /// <summary>
    /// Creates an error object from the passed in string.
    /// </summary>
    public class ErrorCreator
    {
        public static ErrorObject CreateErrorFromJSONString(string jsonString)
        {
            var errorObject = new ErrorObject();
            errorObject = Newtonsoft.Json.JsonConvert.DeserializeObject<ErrorObject>(jsonString);

            return errorObject;
        }
    }

    /// <summary>
    /// Error object with details of the error that occurred.
    /// </summary>
    public class Error
    {
        public int code { get; set; }
        public string message { get; set; }
    }
    /// <summary>
    /// Container object for the Error.
    /// </summary>
    public class ErrorObject
    {
        public Error error { get; set; }
    }
}
