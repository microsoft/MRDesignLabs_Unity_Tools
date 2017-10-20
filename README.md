# MRDL Unity Tools
This repo is where Microsoft's Windows Mixed Reality Design team publishes the Unity tools we use in examples and explorations. These set of tools have been separated into this repo for easy inclusion in your project or repo.  This tools repository is now used as a submodule throughout our MRDesignLabs examples.

# Including as a SubModule
The simplest way to add the tools to your project is via a submodule.  This will create and internal link within your repo to the *MRDesignLab* set of tools.

To do this simply navigate to your Assets folder in your project and use:
 * git submodule add https://github.com/Microsoft/MRDesignLabs_Unity_Tools.git ./MRDesignLab
 
This will create the submodule for Mixed Reality Design Labs Unity tools and allow you to stay up to date with changes to the base toolset or decide not to integrate newer changes.

# Upgrading the Submodule
To upgrade the submodule you'll want to do the following to sync to head, pull and commit:
 * git submodule foreach git checkout HEAD
 * git submodule foreach git pull
 * git add .
 
This should checkout the individual submodules to the latest revision and pull the content.  You will see that they have new changes if you do **git status** in the main repository.  Then you simply add the submodule directories and commit.  Note that you can also decide to sycn a submodule to a specific commit based on your projects dependencies.

# Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
