# Use an official Node.js runtime as a base image
FROM node:18

# Set the working directory
WORKDIR /usr/src/app

# Copy package files and install dependencies
COPY package.json  ./

RUN apt-get update && apt-get install -y curl iputils-ping

RUN npm install

# Copy the rest of the application code
COPY . .

# Expose the port the app runs on
EXPOSE 3000

# Define the command to run your app
CMD [ "npm", "start" ]
