import azure.functions as func
import logging
import os
import joblib
import numpy as np
import json

app = func.FunctionApp(http_auth_level=func.AuthLevel.ANONYMOUS)

@app.route(route="PingTest")
def PingTest(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    name = req.params.get('name')
    if not name:
        try:
            req_body = req.get_json()
        except ValueError:
            pass
        else:
            name = req_body.get('name')

    if name:
        return func.HttpResponse(f"Hello, {name}. This HTTP triggered function executed successfully.")
    else:
        return func.HttpResponse(
             "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response.",
             status_code=200
        )
    
@app.route(route="MLModel")
def MLModel(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    ticker = req.params.get('ticker')
    if not ticker:
        try:
            req_body = req.get_json()
        except ValueError:
            pass
        else:
            ticker = req_body.get('name')

    if ticker:
        price = predict_next_quarter_price(ticker)
        response = {
            "price": price
        }
        return func.HttpResponse(
            json.dumps(response),
            mimetype="application/json"
        )
    else:
        return func.HttpResponse(
             f"Ticker {ticker} not found",
             status_code=400
        )
    

@app.route(route="MLModelResults")
def MLModelResults(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    ticker = req.params.get('ticker')
    if not ticker:
        try:
            req_body = req.get_json()
        except ValueError:
            pass
        else:
            ticker = req_body.get('name')

    if ticker:
        data = {}
        with open(f"./Models/{ticker}/{ticker}_results.json", 'r') as file:
            data = json.load(file)
        
        return func.HttpResponse(
            json.dumps(data),
            mimetype="application/json"
        )
    else:
        return func.HttpResponse(
             f"Ticker {ticker} not found",
             status_code=400
        )
    

def predict_next_quarter_price(ticker: str):
    model_dir = os.path.join('Models', ticker)
    
    try:
        # Load model, scaler, and latest features
        model = joblib.load(os.path.join(model_dir, 'best_svm_model.pkl'))
        scaler = joblib.load(os.path.join(model_dir, 'scaler.pkl'))
        latest_features = joblib.load(os.path.join(model_dir, f'{ticker}_latest_features.pkl'))
        
        # Transform input and make prediction
        scaled_input = scaler.transform(latest_features)
        pred_log = model.predict(scaled_input)
        pred = np.expm1(pred_log)

        return pred[0] if len(pred) == 1 else pred

    except FileNotFoundError as e:
        print(f"[ERROR] File not found for ticker '{ticker}': {e}")
    except Exception as e:
        print(f"[ERROR] Something went wrong with prediction for '{ticker}': {e}")

    return -1