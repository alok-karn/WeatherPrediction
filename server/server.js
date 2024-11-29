const express = require("express");
const bodyParser = require("body-parser");
const tf = require("@tensorflow/tfjs-node");
const fs = require("fs");
const cors = require("cors");

// Load the dataset
async function loadDataset() {
    const data = fs.readFileSync("seattle-weather.csv", "utf-8");
    const lines = data.split("\n").slice(1); // Remove the header
    const features = [];
    const labels = [];

    lines.forEach((line) => {
        const [date, precipitation, temp_max, temp_min, wind, weather] =
            line.split(",");
        features.push([
            parseFloat(temp_min),
            parseFloat(temp_max),
            parseFloat(precipitation),
            parseFloat(wind),
        ]);
        labels.push(getLabel(weather?.trim())); // Encode 'weather' as a numeric label
    });

    return { features, labels };
}

// Convert 'weather' labels into numeric classes
function getLabel(weather) {
    switch (weather) {
        case "drizzle":
            return 0;
        case "fog":
            return 1;
        case "rain":
            return 2;
        case "snow":
            return 3;
        case "sun":
            return 4;
        default:
            return -1;
    }
}

// Define the model
function createModel() {
    const model = tf.sequential();

    // Input layer: 4 features (temp_min, temp_max, precipitation, wind)
    model.add(
        tf.layers.dense({ inputShape: [4], units: 8, activation: "relu" })
    );

    // Hidden layer
    model.add(tf.layers.dense({ units: 16, activation: "relu" }));

    // Output layer: 5 possible weather types (drizzle, fog, rain, snow, sun)
    model.add(tf.layers.dense({ units: 5, activation: "softmax" }));

    model.compile({
        optimizer: tf.train.adam(),
        loss: "sparseCategoricalCrossentropy",
        metrics: ["accuracy"],
    });

    return model;
}

// Train the model
async function trainModel(model, features, labels) {
    const xs = tf.tensor2d(features);
    const ys = tf.tensor1d(labels, "float32");

    // Train the model
    await model.fit(xs, ys, {
        epochs: 50,
        shuffle: true,
        validationSplit: 0.2,
    });
}

const weatherToSkybox = {
    drizzle: "rainySky",
    fog: "foggySky",
    rain: "rainySky",
    snow: "snowySky",
    sun: "sunnySky",
};

// Set up Express server
const app = express();
app.use(bodyParser.json());
app.use(cors());

let model; // Store the trained model

app.post("/train", async (req, res) => {
    const { features, labels } = await loadDataset();

    model = createModel();
    await trainModel(model, features, labels);

    res.json({ message: "Model trained successfully!" });
});

app.get("/api/skybox", async (req, res) => {
    res.json({
        skybox: "sunnySky",
    });
});

// Predict route
app.post("/predict", async (req, res) => {
    const { temp_min, temp_max, precipitation, wind } = req.body;

    if (
        temp_min === undefined ||
        temp_max === undefined ||
        precipitation === undefined ||
        wind === undefined
    ) {
        return res.status(400).json({ error: "Missing required fields" });
    }

    const input = tf.tensor2d([[temp_min, temp_max, precipitation, wind]]);

    const prediction = model.predict(input);
    const predictedIndex = prediction.argMax(1).dataSync()[0]; // Get the index of the highest probability
    const weatherTypes = ["drizzle", "fog", "rain", "snow", "sun"];
    const predictedWeather = weatherTypes[predictedIndex];
    const skyboxName = weatherToSkybox[predictedWeather];

    res.json({ prediction: predictedWeather, skybox: skyboxName });
});

// Start the server
const PORT = 5000;
app.listen(PORT, () => {
    console.log(`Server running on http://localhost:${PORT}`);
});
