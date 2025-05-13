## To Run the Application Locally

### Prerequisites
- [Node.js](https://nodejs.org/) (v18 recommended)
- [Docker](https://www.docker.com/)

### ✅ Installation and Local Deployment Guide
1. **Clone the Repository:**
   ```bash
   git clone https://github.com/your-repo/Automated-Code-Translation-JDB-4310.git
2. **Navigate to the project directory:**
    ```bash
    cd ACT-JDB-4310
### 🐳 Docker Setup
3. Ensure Docker is Installed:
  - Link to Docker Download: [Docker](https://www.docker.com/)

4. Once downloaded, confirm that docker is working:
    ```bash
    docker -v
### ⚙️ Environment Configuration
5. Use .env.example to setup environment file
  - Navigate to ACT-app/Docker
    ```bash
    cd ACT-app/Docker
  - Copy the example *.env* file
    ```bash
    cp .env.example .env
  - Open the .env file and replace **placeholder values** with your actual credentials 
  - For OpenAI API Key, make sure your account associated with the key has enough balance to support the model of choice in ACT-ml/scripts/api.py.

6. Update your /etc/hosts file:
 - Add authentik as an alias to 127.0.0.1
    ```bash
    sudo nano /etc/hosts
 - Add this line at the bottom:
    ```bash
    127.0.0.1 authentik
- Save and close *(CTRL+0,Enter, then CTRL+X)*
### 📁 Docker Compose Setup
7. Navigate to Docker folder
    ```bash
    cd ACT-app/Docker
8. Build and Run the Docker Containers:
    ```bash
    docker compose up --build
9. Verify all containers are running:
    ```bash
    docker ps
### 🔐 Getting Authentik to Work Locally

10. Make sure the authentik and authentik-worker containers are not running:
    ```bash
    docker stop authentik authentik-worker
- Then Run:
  ```bash
  docker exec -it postgresql psql -U authentik -d postgres
11. In the PostgresSQL Shell that is now open, run:
    ```bash
      DROP DATABASE IF EXISTS authentik;
      CREATE DATABASE authentik;
      \q
12. Back in the terminal, run this command: 
    ```bash
    docker exec -it postgresql psql -U authentik -d authentik -f /authentik_dump.sql
13. Restart Authentik Services:
    ```bash
    docker compose restart authentik authentik-worker
- OR Restart manually via Docker dashboard

14. Access Authentik: Open your browser and go to *http://localhost:9000*
### 🚀 Launch the Application
15. Access the application:
  - Open your browser and navigate to:
    ```bash
    http://localhost:3000/