import math
import re
from collections import Counter
from database import get_all_questions_of_user


# Function to calculate the cosine similarity (as defined previously)
def cosine_similarity(vector_a, vector_b):
    dot_prod = sum(x * y for x, y in zip(vector_a, vector_b))
    mag_a = math.sqrt(sum(x * x for x in vector_a))
    mag_b = math.sqrt(sum(x * x for x in vector_b))
    if mag_a == 0 or mag_b == 0:  # To handle DivisionByZero error
        return 0
    return dot_prod / (mag_a * mag_b)

# Function to convert text to a vector of words
def text_to_vector(text):
    words = re.compile(r"[\w']+").findall(text.lower())
    return Counter(words)

# Function to get vector form of a document
def get_vector_for_text(text, feature_set):
    text_vec = text_to_vector(text)
    return [text_vec.get(feature, 0) for feature in feature_set]


def find_most_similar_question(search_term, user_id):
    # Questions (documents)
    documents = get_all_questions_of_user(user_id)

    # Create a set of all unique words in all documents and the query
    unique_words = set()
    for doc in documents + [search_term]:
        unique_words |= set(text_to_vector(doc))

    # Convert documents to vectors
    doc_vectors = [get_vector_for_text(doc, unique_words) for doc in documents]

    # Convert search_term to a vector
    search_vector = get_vector_for_text(search_term, unique_words)

    # Calculate cosine similarity between each document and the search term
    similarities = [cosine_similarity(search_vector, doc_vector) for doc_vector in doc_vectors]

    # Get the most relevant question
    most_relevant_index = similarities.index(max(similarities))
    most_relevant_question = documents[most_relevant_index]

    return most_relevant_question