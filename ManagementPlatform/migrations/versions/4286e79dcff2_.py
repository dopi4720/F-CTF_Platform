"""empty message

Revision ID: 4286e79dcff2
Revises: 53aab1b72b47
Create Date: 2024-11-13 10:04:24.843192

"""
from alembic import op
import sqlalchemy as sa


# revision identifiers, used by Alembic.
revision = '4286e79dcff2'
down_revision = '53aab1b72b47'
branch_labels = None
depends_on = None


def upgrade():
    # ### commands auto generated by Alembic - please adjust! ###
    op.add_column('challenges', sa.Column('user_id', sa.Integer(), nullable=False))
    op.create_foreign_key(None, 'challenges', 'users', ['user_id'], ['id'])
    # ### end Alembic commands ###


def downgrade():
    # ### commands auto generated by Alembic - please adjust! ###
    op.drop_constraint(None, 'challenges', type_='foreignkey')
    op.drop_column('challenges', 'user_id')
    # ### end Alembic commands ###