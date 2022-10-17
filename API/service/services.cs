using API.classes;

namespace API.service;

public class services
{
    public IEnumerable<ImageData> LoadFromDirectory(string folder)
    {
        var files = Directory.GetFiles(folder, "*", searchOption: SearchOption.AllDirectories);
        foreach (var file in files)
        {
            if ((Path.GetExtension(file) != ".jpg") &&
                (Path.GetExtension(file) != ".png") &&
                (Path.GetExtension(file) != ".jpeg")) continue;
            var label = Directory.GetParent(file).Name;

            yield return new ImageData() { ImagePath = file, Label = label };
        }
    }
}