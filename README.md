# performance-tags
 Experimenting with Unity Tag speed in Unity 2021.2.1f1
 
 <B>BetterTag</B> is a component which can be attached to each object you want to tag.
 <UL>
 <LI>Tags are in an enum at the top of BetterTag.cs</LI>
 <LI>Core data structure is Dictionary(BetterTag, HashSet(GameObject))</LI>
 </UL>

My speed tests are built into the project.

 <UL>
 <LI>Changing the tag on every object every frame enough times to be measurably slow</LI>
 <LI>Tests conducted on a pretty stable Xeon X5660 6-core system running Win 10 Pro with 30 GB RAM.</LI>
 </UL>

This project builds to a standalone 640x480 windowed application.

Initial tests indicate that Unity Tag is very fast and my version takes about 1.7x as long. So, not actually "Better". LOL.

![image](https://user-images.githubusercontent.com/39740472/140593734-c6026ca6-c815-4c51-8894-21c3954ae316.png)

![image](https://user-images.githubusercontent.com/39740472/140593739-b5176bfe-56fe-4f92-8883-a6b42dbe7063.png)
