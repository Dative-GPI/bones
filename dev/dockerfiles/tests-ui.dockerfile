FROM node:lts

COPY ./src/Bones.UI /app/src/Bones.UI
WORKDIR /app/src/Bones.UI
RUN npm install
RUN npm link

COPY ./tests/Bones.UI.Tests /app/tests/Bones.UI.Tests
WORKDIR /app/tests/Bones.UI.Tests

RUN npm link @dative-gpi/bones-ui

CMD ["npm", "test"]
