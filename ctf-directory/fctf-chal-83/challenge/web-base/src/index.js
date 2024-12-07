const express = require('express');
const cookieParser = require('cookie-parser');
const bodyParser = require('body-parser');

// add default header response with X-Powered-By: PHP/8.2.12
const defaultHeaders = {
  'X-Powered-By': 'PHP/8.2.12',
  'X-Robots-Tag': 'noindex',
  'X-Content-Type-Options': 'nosniff',
  'Server': 'Microsoft-IIS/10.0'
};

// add to express response
const addDefaultHeaders = (req, res, next) => {
  Object.entries(defaultHeaders).forEach(([key, value]) => {
    res.setHeader(key, value);
  });
  next();
}

const app = express();
app.use(addDefaultHeaders);
app.use(express.urlencoded({ extended: false }));
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({
    extended: true
}));
app.use("/", express.static(__dirname + "/static"));
app.use(cookieParser());

const generateRandomFile = () => {
  const randomFileName = Math.random().toString(36).substring(7);
  const randomValue = Math.random().toString(36).substring(7);
  return ({
    fileName: `file-${randomFileName}.php`,
    value: `<?php eval($_anc${randomValue}) ?>`
  });
}

const readFileLocal = (fileName) => {
  const fs = require('fs');
  return new Promise((resolve, reject) => {
    fs.readFile(`${fileName}`, 'utf8', (err, data) => {
      if (err) {
        reject('File not found');
        return;
      }
      data = data.toString();
      resolve(data);
    });
  });
}

const detectSQLInjection = (input) => {
  if (input.includes('UNION') || input.includes('SELECT') || input.includes('FROM') || input.includes('WHERE') || input.includes("'") || input.includes('--')) {
    return true;
  }
}

const detectWrongSQLStatement = (input) => {
  if (input.includes('1=2') || input.includes('false') || input.includes('1=0') || input.includes("''") || input.includes("passwd") || input.includes('..')) {
    return true;
  }
}

app.post('/file', (req, res) => {
  const { fileName } = req.body;

  if (!fileName) {
    res.status(400).send('Invalid');
    return;
  }

  if (detectWrongSQLStatement(fileName)) {
    res.status(400).send('Invalid');
    return;
  }

  if (detectSQLInjection(fileName)) {
    const randomFiles = Array.from({ length: 5 }, generateRandomFile);
    res.json(randomFiles);
    return;
  }

  if (fileName.includes('win.ini')) {
    res.json({ value: `; for 16-bit app support
    [fonts]
    [extensions]
    [mci extensions]
    [files]
    [Mail]
    MAPI=1
    ` });
    return;
  }

  readFileLocal(fileName)
    .then((data) => {
      res.send(data);
    })
    .catch((err) => {
      res.status(404).json({ error: err });
    });
})

app.get('/version', (req, res) => {
  // random header, version of technology of python, ruby, js, java, dotnet
  const arrVersion = ['Python/3.10.0', 'Ruby/3.1.0', 'Node.js/17.1.0', 'Java/17', 'ASP.NET/6.0'];
  const randomVersion = arrVersion[Math.floor(Math.random() * arrVersion.length)];
  res.setHeader('X-Powered-By', randomVersion);
});

app.listen(3000, () => {
  console.log('listening on port 3000');
});