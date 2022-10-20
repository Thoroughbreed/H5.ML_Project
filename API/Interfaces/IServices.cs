using System.Net;

namespace API.Interfaces;

public interface IServices
{
    /// <summary>
    /// Method for testing the image against the current trained model.
    /// </summary>
    /// <param name="image">Image in bytes encoded in Base64 format</param>
    /// <param name="captcha">If this is part of the training process, set it true. That way the engine doesn't save the image</param>
    /// <returns>A List of Tuples containing the classification (string) and a score percentage (float)</returns>
    Task<List<Tuple<string, float>>> TestImage(byte[]? image, bool captcha = false);
    
    /// <summary>
    /// Re-trains the model if preconditions are matched
    /// </summary>
    /// <returns>HTTP Status Code (424, Failed dependency or 202, accepted)</returns>
    HttpStatusCode ReTrain();
    
    /// <summary>
    /// Hands out an image from the "Unknown" folder in the model
    /// </summary>
    /// <returns>Tuple<Guess, Score, Image(Base64 byte[]), path</returns>
    Task<Tuple<string, float, byte[], string>> Captcha();
    
    /// <summary>
    /// Reply to the AI what the sentient being thinks the image is
    /// </summary>
    /// <param name="reply">Tuple<Classification, Path></param>
    void CaptchaReturn(Tuple<string, string> reply);
    
    /// <summary>
    /// Delete an image not in the classification scope
    /// </summary>
    /// <param name="path">Path of the image</param>
    void DeleteWrong(string path);
}