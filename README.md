GroupByYearThenMonth
====================

Sort the files in the given folder by the year and month the photo was taken

[![Build status](https://ci.appveyor.com/api/projects/status/2bun6p7gxotbb526/branch/master?svg=true)](https://ci.appveyor.com/project/FrancoisBard/groupbyyearthenmonth/branch/master)

Description
===========

Sort the files in the given folder by the year and month the photo was taken.
For each file, it checks if it is an image and has a PropertyTagExifDTOrig PropertyItem tag.
If it does, it moves the file in the correct folder.
(If the directory path was DIR/, the file is moved to DIR/YEAR/MONTH/).
Other files are ignored.
The program doesn't look for files in subfolders.

Why use this program ?
======================

I personaly use it to sort the photos of my son into subfolders before sending them to the grandparents. 
This way i can add some hierarchy to the 10.000+ files I get from the Picasa export.
