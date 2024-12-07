FROM node:latest

RUN mkdir -p /usr/src/app

WORKDIR /usr/src/app

COPY ./src/ .

RUN npm install

RUN echo "FUSEC{d0_not_tru57_corn}" > /home/flag.js

EXPOSE 3000

CMD [ "node", "index.js" ]