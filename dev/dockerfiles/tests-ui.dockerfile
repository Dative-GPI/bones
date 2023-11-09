FROM node:lts

WORKDIR /app

COPY ./src ./src
COPY ./tests ./tests
COPY ./package.json ./package.json

RUN npm install

ENTRYPOINT ["npm", "test", "-w"]