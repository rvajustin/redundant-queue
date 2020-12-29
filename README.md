# Redundant Queue
 
![NuGet Generation](https://github.com/rvajustin/redundant-queue/workflows/NuGet%20Generation/badge.svg)


---

Automatically manage queue storage with redundant queues.  Sometimes queues are fickle or have poor SLAs.  This library permits the use of redundant queues if a primary queue fails.  

---


This is a faux monorepo that combines three assets:
- the core RedundantQueue libraries for managing redundant queues
- unit tests that cover the RedundantQueue libraries
- the integration code for adding RedundantQueue functionality to your ASP.NET Core application


## Getting started
1. Reference the Nuget package
1. Configure your web services
1. Add your redundant queues


## License
All packages in this repository are released under the MIT License.  See [LICENSE](LICENSE) for details.