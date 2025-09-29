<div id="top"></div>

<!-- Readme template from https://github.com/othneildrew/Best-README-Template -->

<!-- PROJECT SHIELDS -->
<!--
*** I'm using markdown "reference style" links for readability.
*** Reference links are enclosed in brackets [ ] instead of parentheses ( ).
*** See the bottom of this document for the declaration of the reference variables
*** for contributors-url, forks-url, etc. This is an optional, concise syntax you may use.
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![GNU Affero General Public License v3.0 License][license-shield]][license-url]



<div align="center">

<h1 align="center">Cloudflare dynamic DNS</h3>

  <p align="center">
    Reliable Cloudflare Record Synchronization for Dynamic IPs.
  </p>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#features">Features</a></li>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project
**Cloudflare Dynamic DNS Updater** is a light, reliable, and automated solution designed to keep your home or office network's public IP address consistently synchronized with a **Cloudflare DNS record**.

This project solves the common problem for users with dynamic IPs (like most residential connections) who want to run a server or service accessible via a fixed domain name. By leveraging the **Cloudflare API**, it automatically detects changes to your public IP address and instantly updates your specified DNS record, ensuring your service is always reachable.

---

### Features
Our Cloudflare Dynamic DNS client is designed for reliability and ease of use, ensuring your services are always reachable.

* **Intelligent Synchronization:** Only updates your DNS records when a **genuine IP address change** is detected, significantly reducing unnecessary Cloudflare API calls.
* **Multi-Record Management:** Easily configure and manage updates for **multiple domains, hosts, and subdomains** all from a single instance.
* **Secure API Integration:** Uses the **official Cloudflare API** to securely and reliably update your DNS A-records.
* **Versatile Deployment:** Built on **.NET** and available as a streamlined **Docker image**, making it simple to deploy on diverse platforms like a Raspberry Pi, NAS, or home server.
* **Robust IP Detection:** Features reliable logic to accurately determine your current public IP address, ensuring precision in every update.

### Built With

* [.NET](https://dotnet.microsoft.com/en-us/)


<!-- GETTING STARTED -->
## Getting Started
Setting up this solution on your local machine is straightforward and will enable you to fully utilize its capabilities. This guide will walk you through the necessary steps to get everything running smoothly.

Before beginning, ensure that your development environment is properly configured. Having the required software and dependencies installed will prevent common issues and streamline the process.

### Prerequisites

### Installation
This installation method utilizes Docker Compose for a streamlined setup. Ensure you have Docker and Docker Compose installed on your system.

1.  **Create a `docker-compose.yml` file:**
    Create a new file named `docker-compose.yml` in a directory of your choice. Copy and paste the following content into it:

    ```yaml
    version: '3.4'
    name: cloudflare-dynamic-dns
    services:
      cloudflare.dynamic.dns:
        container_name: "cloudflare-dynamic-dns"
        image: ghcr.io/jellebuning/cloudflare-dynamic-dns
        environment:
          # Required
          CF_API_TOKEN: <cloudflare_api_token>
          CF_DOMAIN_NAMES: domain.com, sub.domain.com

          # Optional
          INTERVAL_MINUTES: <interval> # defaults to 15 if variable is not set.
    ```

2.  **Run Docker Compose:**
    In the same directory as your `docker-compose.yml` file, execute the following command:

    ```bash
    docker-compose up -d
    ```

    This command will download the necessary images, create the containers, and start them in detached mode.

<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request


<!-- LICENSE -->
## License
Distributed under the GNU Affero General Public License v3.0 License. See `LICENSE` for more information.


<p align="right">(<a href="#top">back to top</a>)</p>



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/JelleBuning/cloudflare-dynamic-dns.svg?style=for-the-badge
[contributors-url]: https://github.com/JelleBuning/cloudflare-dynamic-dns/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/JelleBuning/cloudflare-dynamic-dns.svg?style=for-the-badge
[forks-url]: https://github.com/JelleBuning/cloudflare-dynamic-dns/network/members
[stars-shield]: https://img.shields.io/github/stars/JelleBuning/cloudflare-dynamic-dns.svg?style=for-the-badge
[stars-url]: https://github.com/JelleBuning/cloudflare-dynamic-dns/stargazers
[issues-shield]: https://img.shields.io/github/issues/JelleBuning/cloudflare-dynamic-dns.svg?style=for-the-badge
[issues-url]: https://github.com/JelleBuning/cloudflare-dynamic-dns/issues
[license-shield]: https://img.shields.io/github/license/JelleBuning/cloudflare-dynamic-dns.svg?style=for-the-badge
[license-url]: https://github.com/JelleBuning/cloudflare-dynamic-dns/blob/master/LICENSE
