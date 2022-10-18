![Build succeeded][build-shield]
![Test passing][test-shield]
[![Issues][issues-shield]][issues-url]
[![Issues][closed-shield]][issues-url]
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![License][license-shield]][license-url]
# Machine Learning - Final Project
<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#summary">Summary</a>
      <ul>
        <li><a href="#case">Case</a></li>
	      <li><a href="#project-structure">Project Structure</a></li>
      </ul>
    </li>
    <li>
      <a href="#project-overview">Project Overview</a>
      <ul>
        <li><a href="#application-layout">Application Layout</a></li>
        <li><a href="#uml">UML diagram</a></li>
      </ul>
    </li>
    <li><a href="#changelog">Changelog</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
  </ol>
</details>

## Summary
This is a demo project showing a proof of concept use for Machine Learning at my fifth semester.
The project is not supposed to be very accurate, but only to showcase the use of ML in a real world application, using real world training data.
<p align="right">(<a href="#top">back to top</a>)</p>

### Case
#### Event agency want to improve their premises
A local event agency contacted our firm to get a machine learning model of the use of their location and parking place. 
They would like an AI to monitor their area for 6 months to show how many people visit them, and how many cars park there. 
The purpose of this task is to:
1. Take an image from a security camera and process it
2. Locate the wanted objects on the given image (people + vehicles)
3. ~~Summarize the amount of recognized objects on the image~~
   - I ran into several opstacles using ML.Net, and instead of switching to Python (the course was primarily about ML.Net) I decided to go with a different approach
4. Locate a known object on the image and return it as a string, if no object found return "Not found" and save the image in a learning-folder to use it later for a captcha-style learning process.
5. Create logic for further training. Save unknown images in `./unknown` and save new known images in the respective folder.
   - After *n* new files in the known folder, re-run the training, including the new images.
   - Have a captcha-style "what is this" presented for users, to help give the model more accuracy.
     - Make a fail-safe logic for images that eg. is marked "person" when the confidence of the current model is <20 to avoid trolls
7. Output
   - ~~Return the image via the WebAPI to the sender with a mark around the identified objects *(If I can make it work on macOS, and if I have time for it)*~~
   - Return a string/JSON from the WebAPI to the sender with ~~a summarization of amount of identified objects~~ the identified object, or *Not found* if no known object was located.
   - Return a string in a console application from the image with a summarization *(Failsafe model if the API doesn't get completed in time)*
<p align="right">(<a href="#top">back to top</a>)</p>

### Project structure
```
ML Project
│   README.md
│   LICENSE.txt    
└───API
│   │───Program.cs
│   └───data
│   │   │───test        - Test dataset
│   │   │───train       - Train dataset
│   │   │───val         - Validation dataset
│   │   └───unknown     - Unknown (captcha)
│   │
│   └───service
│   │   │───Service.cs  - Image classification services
│   │   └───IService.cs
│   │
│   └───trainer
│       │───Tainer.cs   - Re-train (generation incrementer)
│       └───ITrainer.cs
│   
└───Frontend
    │   404
```

<p align="right">(<a href="#top">back to top</a>)</p>

## Project overview
<p align="right">(<a href="#top">back to top</a>)</p>

### Application layout
<p align="right">(<a href="#top">back to top</a>)</p>

### UML
<p align="right">(<a href="#top">back to top</a>)</p>

### Changelog
| Version | Change |
|-|-|
| 0.0.0 | Project |
| 0.0.1 | API Startup |
| 0.0.2 | Finalized debugging endpoints. Can start training from API |
| 0.0.3 | Added testing one random image |
| 0.1.0 | Added service to assess an image through the API |
| 0.1.1 | Refactored code |
| 0.1.2 | Edited deprecated methods, removed debugging method calls |
| 0.1.3 | Removed "Test random image" endpoint | 
<p align="right">(<a href="#top">back to top</a>)</p>

### Roadmap
- [X] Gather enough data (and sanitize it)
- [X ] Generate the machine model and verify the accuracy of it (30% confidence is my minimum as PoC)
- [ ] ~~Create the logic behind the object recognition~~
- [X] Test the logic, and see if it is accurate/satisfying enough
  - [X] Test the identification logic (is it a person, a car, a bike, a tree or other?)
  - [ ] Test the re-train logic to see if we can make it more accurate over time
  - [ ] Test the fail-safe logic, avoiding trolls (an image of a flower marked as a car)
- [ ] Create frontend
  - [ ] Console all-in-one *(smoke test)*
  - [X] Razor + WebAPI returning a string/JSON
  - [ ] ~~Razor + WebAPI returning an image with the identified objects *(marked on the original image)*~~
  - [ ] ~~Razor + WebAPI returning an array of images with each of the identified objects cropped out of the original image~~
<p align="right">(<a href="#top">back to top</a>)</p>

### License
* Software: GPLv3
<p align="right">(<a href="#top">back to top</a>)</p>


### Contact
Jan Andreasen - jan@tved.it

[![Twitter][twitter-shield]][twitter-url]

Project Link: [https://github.com/Thoroughbreed/H5.ML_Project](https://github.com/Thoroughbreed/H5.ML_Project)
<p align="right">(<a href="#top">back to top</a>)</p>


<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[build-shield]: https://img.shields.io/badge/Build-failed-red.svg
[test-shield]: https://img.shields.io/badge/Tests-failed-red.svg
[contributors-shield]: https://img.shields.io/github/contributors/Thoroughbreed/H5.ML_Project.svg?style=badge
[contributors-url]: https://github.com/Thoroughbreed/H5.ML_Project/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/Thoroughbreed/H5.ML_Project.svg?style=badge
[forks-url]: https://github.com/Thoroughbreed/H5.ML_Project/network/members
[issues-shield]: https://img.shields.io/github/issues/Thoroughbreed/H5.ML_Project.svg?style=badge
[closed-shield]: https://img.shields.io/github/issues-closed/Thoroughbreed/H5.ML_Project?label=%20
[issues-url]: https://github.com/Thoroughbreed/H5.ML_Project/issues
[license-shield]: https://img.shields.io/github/license/Thoroughbreed/H5.ML_Project.svg?style=badge
[license-url]: https://github.com/Thoroughbreed/H5.ML_Project/blob/master/LICENSE
[twitter-shield]: https://img.shields.io/twitter/follow/andreasen_jan?style=social
[twitter-url]: https://twitter.com/andreasen_jan
