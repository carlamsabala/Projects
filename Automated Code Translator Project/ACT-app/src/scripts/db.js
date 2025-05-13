const nano = require('nano')('http://admin:adminpassword@couchdb:5984');  // Replace with your actual credentials

const usersDB = nano.db.use('users');
const translationsDB = nano.db.use('translations');

// ✅ Function to Create Databases if They Don't Exist
async function createDatabase(dbName) {
    try {
        const dbList = await nano.db.list();
        if (!dbList.includes(dbName)) {
            await nano.db.create(dbName);
            console.log(`✅ Database ${dbName} created successfully.`);
        } else {
            console.log(`✅ Database ${dbName} already exists.`);
        }
    } catch (error) {
        console.error(`❌ Error creating database ${dbName}:`, error.reason || error.message);
    }
}

// Initialize databases
createDatabase('users');
createDatabase('translations');

module.exports = { usersDB, translationsDB };
