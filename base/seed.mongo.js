const database = db.getSiblingDB("phishing_poc");

const shortenersCollection = database.getCollection("shorteners");
const companiesCollection = database.getCollection("companies");
const templatesCollection = database.getCollection("templates");

const knownShortenerDomains = [
  "bit.ly",
  "tinyurl.com",
  "t.co",
  "ow.ly",
  "is.gd",
  "buff.ly",
  "rebrand.ly",
  "cutt.ly",
  "shorturl.at",
  "tiny.cc",
  "rb.gy",
  "t.ly",
  "v.gd",
  "bl.ink",
  "s.id",
  "lnkd.in",
  "goo.gl",
  "tr.im",
  "qr.ae",
  "adf.ly"
];

const testCompanies = [
  "BancoExemplo",
  "LojaExemplo",
  "EnergiaExemplo",
  "TelecomExemplo",
  "SeguroExemplo",
  "Cartao Exemplo",
  "Correio Exemplo",
  "Streaming Exemplo",
  "Mercado Exemplo",
  "Banco Nacional Exemplo"
];

function criarTemplate(empresa) {
  const dominio = empresa.replace(/\s+/g, "").toLowerCase() + ".com.br";
  return {
    company: empresa,
    official_domains: [dominio, "www." + dominio]
  };
}

try {
  shortenersCollection.deleteMany({});
  companiesCollection.deleteMany({});
  templatesCollection.deleteMany({});

  const shortenerDomains = knownShortenerDomains.map(domain => ({
    domain,
    patterns: [domain + "/*"]
  }));

  for (let index = 1; index <= 180; index++) {
    const domain = "curto" + String(index).padStart(4, "0") + ".exemplo";
    shortenerDomains.push({
      domain,
      patterns: [domain + "/*"]
    });
  }

  const companyNames = [...testCompanies];
  for (let index = 1; index <= 4990; index++) {
    companyNames.push("EmpresaSintetica" + String(index).padStart(4, "0"));
  }

  const templates = testCompanies.map(criarTemplate);
  for (let index = 1; index <= 4990; index++) {
    const empresa = "EmpresaSintetica" + String(index).padStart(4, "0");
    const dominio = "empresasintetica" + String(index).padStart(4, "0") + ".com.br";
    templates.push({
      company: empresa,
      official_domains: [dominio, "www." + dominio]
    });
  }

  shortenersCollection.insertOne({ list: shortenerDomains });
  companiesCollection.insertOne({ companies: companyNames });
  templatesCollection.insertMany(templates);
  templatesCollection.createIndex(
    { company: 1 },
    { unique: true, name: "company_unique" }
  );

  const shortenersDocument = shortenersCollection.findOne();
  const companiesDocument = companiesCollection.findOne();
  const templateNames = templatesCollection.distinct("company");
  const companySet = new Set(companiesDocument.companies);
  const templateSet = new Set(templateNames);
  const missingTemplates = companiesDocument.companies.filter(company => !templateSet.has(company));
  const orphanTemplates = templateNames.filter(company => !companySet.has(company));
  const uniqueCompanyIndexExists = templatesCollection.getIndexes().some(index =>
    index.name === "company_unique"
    && index.unique === true
    && index.key.company === 1
    && Object.keys(index.key).length === 1
  );

  const counts = {
    shorteners: shortenersDocument?.list?.length ?? 0,
    companies: companiesDocument?.companies?.length ?? 0,
    templates: templatesCollection.countDocuments({})
  };

  print("Contagens: " + counts.shorteners + " / " + counts.companies + " / " + counts.templates);

  const baseValida = counts.shorteners === 200
    && counts.companies === 5000
    && counts.templates === 5000
    && missingTemplates.length === 0
    && orphanTemplates.length === 0
    && uniqueCompanyIndexExists;

  print(baseValida ? "BASE OK" : "BASE INVALIDA");
  if (!baseValida) {
    print("Templates ausentes: " + missingTemplates.length);
    print("Templates sem empresa no index: " + orphanTemplates.length);
    print("Índice único company: " + (uniqueCompanyIndexExists ? "presente" : "ausente"));
    quit(1);
  }
} catch (error) {
  console.error(error);
  print("BASE INVALIDA");
  quit(1);
}
