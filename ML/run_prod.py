import papermill as pm

# Define input and output notebook paths
input_notebook = "ML_Prod.ipynb"  # Replace with your actual notebook name
output_notebook = "ML_Prod_output.ipynb"

try:
    # Execute the notebook
    pm.execute_notebook(
        input_path=input_notebook,
        output_path=output_notebook,
        log_output=True,        # Print cell outputs to console
        progress_bar=True       # Show progress
    )
    print(f"Notebook executed successfully. Output saved to {output_notebook}")
except Exception as e:
    print(f"Error executing notebook: {str(e)}")